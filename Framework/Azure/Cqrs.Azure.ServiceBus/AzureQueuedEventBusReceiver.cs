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
using System.Threading;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureQueuedEventBusReceiver<TAuthenticationToken> : AzureEventBusReceiver<TAuthenticationToken>
	{
		protected static ConcurrentDictionary<string, ConcurrentQueue<IEvent<TAuthenticationToken>>> QueueTracker { get; private set; }

		protected ReaderWriterLockSlim QueueTrackerLock { get; private set; }

		public AzureQueuedEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper)
		{
			QueueTracker = new ConcurrentDictionary<string, ConcurrentQueue<IEvent<TAuthenticationToken>>>();
			QueueTrackerLock = new ReaderWriterLockSlim();
		}

		protected override void ReceiveEvent(BrokeredMessage message)
		{
			try
			{
				Logger.LogDebug(string.Format("An event message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBody<string>();
				IEvent<TAuthenticationToken> @event = MessageSerialiser.DeserialiseEvent(messageBody);

				CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
				Logger.LogInfo(string.Format("An event message arrived with the id '{0}' was of type {1}.", message.MessageId, @event.GetType().FullName));

				Type eventType = @event.GetType();

				string targetQueueName = eventType.FullName;

				try
				{
					object rsn = eventType.GetProperty("Rsn").GetValue(@event, null);
					targetQueueName = string.Format("{0}.{1}", targetQueueName, rsn);
				}
				catch
				{
					Logger.LogDebug(string.Format("An event message arrived with the id '{0}' was of type {1} but with no Rsn property.", message.MessageId, eventType));
					// Do nothing if there is no rsn. Just use @event type name
				}

				CreateQueueAndAttachListenerIfNotExist(targetQueueName);
				EnqueueEvent(targetQueueName, @event);

				// remove the original message from the incoming queue
				message.Complete();

				Logger.LogDebug(string.Format("An event message arrived and was processed with the id '{0}'.", message.MessageId));
			}
			catch (Exception exception)
			{
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
				message.Abandon();
			}
		}

		private void EnqueueEvent(string targetQueueName, IEvent<TAuthenticationToken> @event)
		{
			var queue = QueueTracker.GetOrAdd(targetQueueName, new ConcurrentQueue<IEvent<TAuthenticationToken>>());
			queue.Enqueue(@event);
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
						QueueTracker.TryAdd(queueName, new ConcurrentQueue<IEvent<TAuthenticationToken>>());
						new Thread(() =>
						{
							Thread.CurrentThread.Name = queueName;
							DequeuAndProcessEvent(queueName);
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

		protected void DequeuAndProcessEvent(string queueName)
		{
			SpinWait.SpinUntil
			(
				() =>
				{
					try
					{
						ConcurrentQueue<IEvent<TAuthenticationToken>> queue;
						if (QueueTracker.TryGetValue(queueName, out queue))
						{
							while (!queue.IsEmpty)
							{
								IEvent<TAuthenticationToken> @event;
								if (queue.TryDequeue(out @event))
								{
									try
									{
										CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
									}
									catch (Exception exception)
									{
										Logger.LogError(string.Format("Trying to set the CorrelationId from the event type {1} for a request for the queue '{0}' failed.", queueName, @event.GetType()), exception: exception);
									}
									try
									{
										AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);
									}
									catch (Exception exception)
									{
										Logger.LogError(string.Format("Trying to set the AuthenticationToken from the event type {1} for a request for the queue '{0}' failed.", queueName, @event.GetType()), exception: exception);
									}
									try
									{
										ReceiveEvent(@event);
									}
									catch (Exception exception)
									{
										Logger.LogError(string.Format("Processing the event type {1} for a request for the queue '{0}' failed.", queueName, @event.GetType()), exception: exception);
										queue.Enqueue(@event);
									}
								}
								else
									Logger.LogDebug(string.Format("Trying to dequeue a event from the queue '{0}' failed.", queueName));
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

					// Always return false to keep this spinning.
					return false;
				}
			);
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