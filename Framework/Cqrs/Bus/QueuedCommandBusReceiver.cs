#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
using Cqrs.Messages;

namespace Cqrs.Bus
{
	public abstract class QueuedCommandBusReceiver<TAuthenticationToken> : ICommandReceiver<TAuthenticationToken>
	{
		protected static ConcurrentDictionary<string, ConcurrentQueue<ICommand<TAuthenticationToken>>> QueueTracker { get; private set; }

		protected ReaderWriterLockSlim QueueTrackerLock { get; private set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		protected abstract IDictionary<Type, IList<Action<IMessage>>> Routes { get; }

		protected QueuedCommandBusReceiver(IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger)
		{
			QueueTracker = new ConcurrentDictionary<string, ConcurrentQueue<ICommand<TAuthenticationToken>>>();
			QueueTrackerLock = new ReaderWriterLockSlim();
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
		}

		protected virtual void EnqueueCommand(string targetQueueName, ICommand<TAuthenticationToken> command)
		{
			var queue = QueueTracker.GetOrAdd(targetQueueName, new ConcurrentQueue<ICommand<TAuthenticationToken>>());
			queue.Enqueue(command);
		}

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

		protected virtual void DequeuAndProcessCommand(string queueName)
		{
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
					Thread.Sleep(100);
				}
				catch (Exception exception)
				{
					Logger.LogError(string.Format("Dequeuing and processing a request for the queue '{0}' failed.", queueName), exception: exception);
				}
			}
		}

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

		public virtual void ReceiveCommand(ICommand<TAuthenticationToken> command)
		{
			CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
			AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);

			IList<Action<IMessage>> handlers;
			if (Routes.TryGetValue(command.GetType(), out handlers))
			{
				if (handlers.Count != 1)
					throw new InvalidOperationException("Cannot send to more than one handler");
				handlers.Single()(command);
			}
			else
			{
				throw new InvalidOperationException("No handler registered");
			}
		}
	}
}