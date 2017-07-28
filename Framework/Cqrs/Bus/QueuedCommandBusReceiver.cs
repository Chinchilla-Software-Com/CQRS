#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cqrs.Authentication;
using Cqrs.Commands;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	/// <summary>
	/// Receives instances of a <see cref="ICommand{TAuthenticationToken}"/> from the command bus, places them into one of several internal concurrent queues and then processes the commands one at a time per queue.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public abstract class QueuedCommandBusReceiver<TAuthenticationToken> : ICommandReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// The queues keyed by an identifier.
		/// </summary>
		protected static ConcurrentDictionary<string, ConcurrentQueue<ICommand<TAuthenticationToken>>> QueueTracker { get; private set; }

		/// <summary>
		/// A <see cref="ReaderWriterLockSlim"/> for providing a lock mechanism around the main <see cref="QueueTracker"/>.
		/// </summary>
		protected ReaderWriterLockSlim QueueTrackerLock { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IBusHelper"/>
		/// </summary>
		protected IBusHelper BusHelper { get; private set; }

		/// <summary>
		/// Gets or sets the routes or handlers that will be executed as the commands arrive.
		/// </summary>
		protected abstract IDictionary<Type, IList<Action<IMessage>>> Routes { get; }

		/// <summary>
		/// Instantiates a new instance of <see cref="QueuedCommandBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		protected QueuedCommandBusReceiver(IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IConfigurationManager configurationManager, IBusHelper busHelper)
		{
			QueueTracker = new ConcurrentDictionary<string, ConcurrentQueue<ICommand<TAuthenticationToken>>>();
			QueueTrackerLock = new ReaderWriterLockSlim();
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
			ConfigurationManager = configurationManager;
			BusHelper = busHelper;
		}

		/// <summary>
		/// Places the provided <paramref name="command"/> into the appropriate queue in the <see cref="QueueTracker"/>.
		/// </summary>
		/// <param name="targetQueueName">The name of the target queue to place the command into</param>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> to handle.</param>
		protected virtual void EnqueueCommand(string targetQueueName, ICommand<TAuthenticationToken> command)
		{
			var queue = QueueTracker.GetOrAdd(targetQueueName, new ConcurrentQueue<ICommand<TAuthenticationToken>>());
			queue.Enqueue(command);
		}

		/// <summary>
		/// Checks if the queue exists, if it doesn't it creates a new queue in <see cref="QueueTracker"/> and then starts a separate <see cref="Thread"/> running <see cref="DequeuAndProcessCommand"/>.
		/// </summary>
		/// <param name="queueName">The name of the queue.</param>
		protected virtual void CreateQueueAndAttachListenerIfNotExist(string queueName)
		{
			if (!QueueTracker.ContainsKey(queueName))
			{
				QueueTrackerLock.EnterWriteLock();
				try
				{
					if (!QueueTracker.ContainsKey(queueName))
					{
						QueueTracker.TryAdd(queueName, new ConcurrentQueue<ICommand<TAuthenticationToken>>());
						new Thread(() =>
						{
							Thread.CurrentThread.Name = queueName;
							DequeuAndProcessCommand(queueName);
						}).Start();
					}
				}
				catch (Exception exception)
				{
					Logger.LogError(string.Format("Processing a request to start a thread for the queue '{0}' failed.", queueName), exception: exception);
				}
				finally
				{
					QueueTrackerLock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Infinitely runs a loop checking if the queue exists in <see cref="QueueTracker"/>
		/// and then dequeues <see cref="ICommand{TAuthenticationToken}"/> one at a time, pausing for 0.1 seconds between loops.
		/// </summary>
		/// <param name="queueName">The name of the queue.</param>
		protected virtual void DequeuAndProcessCommand(string queueName)
		{
			long loop = long.MinValue;
			while (true)
			{
				try
				{
					ConcurrentQueue<ICommand<TAuthenticationToken>> queue;
					if (QueueTracker.TryGetValue(queueName, out queue))
					{
						while (!queue.IsEmpty)
						{
							ICommand<TAuthenticationToken> command;
							if (queue.TryDequeue(out command))
							{
								try
								{
									CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
								}
								catch (Exception exception)
								{
									Logger.LogError(string.Format("Trying to set the CorrelationId from the command type {1} for a request for the queue '{0}' failed.", queueName, command.GetType()), exception: exception);
								}
								try
								{
									AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);
								}
								catch (Exception exception)
								{
									Logger.LogError(string.Format("Trying to set the AuthenticationToken from the command type {1} for a request for the queue '{0}' failed.", queueName, command.GetType()), exception: exception);
								}
								try
								{
									ReceiveCommand(command);
								}
								catch (Exception exception)
								{
									Logger.LogError(string.Format("Processing the command type {1} for a request for the queue '{0}' failed.", queueName, command.GetType()), exception: exception);
									queue.Enqueue(command);
								}
							}
							else
								Logger.LogDebug(string.Format("Trying to dequeue a command from the queue '{0}' failed.", queueName));
						}
					}
					else
						Logger.LogDebug(string.Format("Trying to find the queue '{0}' failed.", queueName));

					if (loop++ % 5 == 0)
						Thread.Yield();
					else
						Thread.Sleep(100);
					if (loop == long.MaxValue)
						loop = long.MinValue;
				}
				catch (Exception exception)
				{
					Logger.LogError(string.Format("Dequeuing and processing a request for the queue '{0}' failed.", queueName), exception: exception);
				}
			}
		}

		/// <summary>
		/// The current number of queues in <see cref="QueueTracker"/>.
		/// </summary>
		public int QueueCount
		{
			get
			{
				QueueTrackerLock.EnterReadLock();
				try
				{
					return QueueTracker.Count;
				}
				finally
				{
					QueueTrackerLock.ExitReadLock();
				}
			}
		}

		/// <summary>
		/// Gets the names of all queues in <see cref="QueueTracker"/>.
		/// </summary>
		public ICollection<string> QueueNames
		{
			get
			{
				QueueTrackerLock.EnterReadLock();
				try
				{
					return QueueTracker.Keys;
				}
				finally
				{
					QueueTrackerLock.ExitReadLock();
				}
			}
		}

		#region Implementation of ICommandReceiver<TAuthenticationToken>

		/// <summary>
		/// Receives a <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		public virtual bool? ReceiveCommand(ICommand<TAuthenticationToken> command)
		{
			CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);

			Type commandType = command.GetType();

			bool response = true;
			bool isRequired = BusHelper.IsEventRequired(commandType);

			IList<Action<IMessage>> handlers;
			if (Routes.TryGetValue(commandType, out handlers))
			{
				if (handlers != null)
				{
					if (handlers.Count != 1)
						throw new MultipleCommandHandlersRegisteredException(commandType);
					if (handlers.Count == 1)
						handlers.Single()(command);
					else if (isRequired)
						throw new NoCommandHandlerRegisteredException(commandType);
					else
						response = false;
				}
				else if (isRequired)
					throw new NoCommandHandlerRegisteredException(commandType);
				else
					response = false;
			}
			else if (isRequired)
				throw new NoCommandHandlerRegisteredException(commandType);
			else
				response = false;
			return response;
		}

		#endregion

		#region Implementation of ICommandReceiver

		/// <summary>
		/// Starts listening and processing instances of <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		public abstract void Start();

		#endregion
	}
}