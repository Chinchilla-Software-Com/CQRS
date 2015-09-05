using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureQueuedCommandBusReceiver<TAuthenticationToken> : AzureCommandBusReceiver<TAuthenticationToken>
	{
		protected static ConcurrentDictionary<string, ConcurrentQueue<ICommand<TAuthenticationToken>>> QueueTracker { get; private set; }

		protected ReaderWriterLockSlim QueueTrackerLock { get; private set; }

		public AzureQueuedCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger)
		{
			QueueTracker = new ConcurrentDictionary<string, ConcurrentQueue<ICommand<TAuthenticationToken>>>();
			QueueTrackerLock = new ReaderWriterLockSlim();
		}

		protected override void ReceiveCommand(BrokeredMessage message)
		{
			// get message Type,
			ICommand<TAuthenticationToken> command = MessageSerialiser.DeserialiseCommand(message.GetBody<string>());
			Type commandType = command.GetType();

			string targetQueueName = commandType.FullName;

			try
			{
				object rsn = commandType.GetProperty("Rsn").GetValue(command, null);
				targetQueueName = string.Format("{0}.{1}", targetQueueName, rsn);
			}
			catch
			{
				Logger.LogDebug(string.Format("Received a command of type '{0}' with no Rsn property.", commandType));
				// Do nothing if there is no rsn. Just use command type name
			}

			CreateQueueAndAttachListenerIfNotExist(targetQueueName);
			EnqueueCommand(targetQueueName, command);

			// remove the original message from the incoming queue
			message.Complete();
		}

		private void EnqueueCommand(string targetQueueName, ICommand<TAuthenticationToken> command)
		{
			var queue = QueueTracker.GetOrAdd(targetQueueName, new ConcurrentQueue<ICommand<TAuthenticationToken>>());
			queue.Enqueue(command);
		}

		protected void CreateQueueAndAttachListenerIfNotExist(string queueName)
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

		protected void DequeuAndProcessCommand(string queueName)
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
	}
}