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
using System.Text;
using System.Threading;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.ServiceBus.Messaging;
using EventData = Microsoft.ServiceBus.Messaging.EventData;
using SpinWait = Cqrs.Infrastructure.SpinWait;

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

		protected override void ReceiveEvent(PartitionContext context, EventData eventData)
		{
			// Do a manual 10 try attempt with back-off
			for (int i = 0; i < 10; i++)
			{
				try
				{
					Logger.LogDebug(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}'.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset));
					string messageBody = Encoding.UTF8.GetString(eventData.GetBytes());
					IEvent<TAuthenticationToken> @event = MessageSerialiser.DeserialiseEvent(messageBody);

					CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
					Logger.LogInfo(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' was of type {3}.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset, @event.GetType().FullName));

					Type eventType = @event.GetType();

					string targetQueueName = eventType.FullName;

					try
					{
						object rsn = eventType.GetProperty("Rsn").GetValue(@event, null);
						targetQueueName = string.Format("{0}.{1}", targetQueueName, rsn);
					}
					catch
					{
						Logger.LogDebug(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' was of type {3} but with no Rsn property.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset, eventType));
						// Do nothing if there is no rsn. Just use @event type name
					}

					CreateQueueAndAttachListenerIfNotExist(targetQueueName);
					EnqueueEvent(targetQueueName, @event);

					// remove the original message from the incoming queue
					context.CheckpointAsync(eventData);

					Logger.LogDebug(string.Format("An event message arrived and was processed with the partition key '{0}', sequence number '{1}' and offset '{2}'.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset));
					return;
				}
				catch (Exception exception)
				{
					// Indicates a problem, unlock message in queue
					Logger.LogError(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' but failed to be process.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset), exception: exception);

					switch (i)
					{
						case 0:
						case 1:
							// 10 seconds
							Thread.Sleep(10 * 1000);
							break;
						case 2:
						case 3:
							// 30 seconds
							Thread.Sleep(30 * 1000);
							break;
						case 4:
						case 5:
						case 6:
							// 1 minute
							Thread.Sleep(60 * 1000);
							break;
						case 7:
						case 8:
						case 9:
							// 3 minutes
							Thread.Sleep(3 * 60 * 1000);
							break;
					}
				}
			}
			// Eventually just accept it
			context.CheckpointAsync(eventData);
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
				},
				sleepInMilliseconds: 1000
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