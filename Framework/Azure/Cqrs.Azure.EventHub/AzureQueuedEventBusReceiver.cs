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
using System.Text;
using System.Threading;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
#if NET452
using Microsoft.ServiceBus.Messaging;
using EventData = Microsoft.ServiceBus.Messaging.EventData;
#endif
#if NETSTANDARD2_0
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using EventData = Microsoft.Azure.EventHubs.EventData;
#endif
using SpinWait = Cqrs.Infrastructure.SpinWait;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A concurrent implementation of <see cref="AzureEventBusReceiver{TAuthenticationToken}"/> that resides in memory.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedEventBusReceiver<TAuthenticationToken> : AzureEventBusReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// Tracks all queues.
		/// </summary>
		protected static ConcurrentDictionary<string, ConcurrentQueue<IEvent<TAuthenticationToken>>> QueueTracker { get; private set; }

		/// <summary>
		/// Gets the <see cref="ReaderWriterLockSlim"/>.
		/// </summary>
		protected ReaderWriterLockSlim QueueTrackerLock { get; private set; }

		/// <summary>
		/// Receives an <see cref="EventData"/> message from the event bus, identifies a key and queues it accordingly.
		/// </summary>
		public AzureQueuedEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IHashAlgorithmFactory hashAlgorithmFactory, IAzureBusHelper<TAuthenticationToken> azureBusHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, hashAlgorithmFactory, azureBusHelper)
		{
			QueueTracker = new ConcurrentDictionary<string, ConcurrentQueue<IEvent<TAuthenticationToken>>>();
			QueueTrackerLock = new ReaderWriterLockSlim();
		}

		/// <summary>
		/// Receives an <see cref="EventData"/> message from the event bus, identifies a key and queues it accordingly.
		/// </summary>
		protected override void ReceiveEvent(PartitionContext context, EventData eventData)
		{
			// Do a manual 10 try attempt with back-off
			for (int i = 0; i < 10; i++)
			{
				try
				{
#if NET452
					Logger.LogDebug(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}'.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset));
#endif
#if NETSTANDARD2_0
					Logger.LogDebug(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}'.", eventData.SystemProperties.PartitionKey, eventData.SystemProperties.SequenceNumber, eventData.SystemProperties.Offset));
#endif
#if NET452
					string messageBody = Encoding.UTF8.GetString(eventData.GetBytes());
#endif
#if NETSTANDARD2_0
					string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
#endif
					IEvent<TAuthenticationToken> @event = MessageSerialiser.DeserialiseEvent(messageBody);

					CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
#if NET452
					Logger.LogInfo(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' was of type {3}.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset, @event.GetType().FullName));
#endif
#if NETSTANDARD2_0
					Logger.LogInfo(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' was of type {3}.", eventData.SystemProperties.PartitionKey, eventData.SystemProperties.SequenceNumber, eventData.SystemProperties.Offset, @event.GetType().FullName));
#endif

					Type eventType = @event.GetType();

					string targetQueueName = eventType.FullName;

					try
					{
						object rsn = eventType.GetProperty("Rsn").GetValue(@event, null);
						targetQueueName = string.Format("{0}.{1}", targetQueueName, rsn);
					}
					catch
					{
#if NET452
						Logger.LogDebug(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' was of type {3} but with no Rsn property.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset, eventType));
#endif
#if NETSTANDARD2_0
						Logger.LogDebug(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' was of type {3} but with no Rsn property.", eventData.SystemProperties.PartitionKey, eventData.SystemProperties.SequenceNumber, eventData.SystemProperties.Offset, eventType));
#endif
						// Do nothing if there is no rsn. Just use @event type name
					}

					CreateQueueAndAttachListenerIfNotExist(targetQueueName);
					EnqueueEvent(targetQueueName, @event);

					// remove the original message from the incoming queue
					context.CheckpointAsync(eventData);

#if NET452
					Logger.LogDebug(string.Format("An event message arrived and was processed with the partition key '{0}', sequence number '{1}' and offset '{2}'.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset));
#endif
#if NETSTANDARD2_0
					Logger.LogDebug(string.Format("An event message arrived and was processed with the partition key '{0}', sequence number '{1}' and offset '{2}'.", eventData.SystemProperties.PartitionKey, eventData.SystemProperties.SequenceNumber, eventData.SystemProperties.Offset));
#endif
					return;
				}
				catch (Exception exception)
				{
					// Indicates a problem, unlock message in queue
#if NET452
					Logger.LogError(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' but failed to be process.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset), exception: exception);
#endif
#if NETSTANDARD2_0
					Logger.LogError(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' but failed to be process.", eventData.SystemProperties.PartitionKey, eventData.SystemProperties.SequenceNumber, eventData.SystemProperties.Offset), exception: exception);
#endif

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

		/// <summary>
		/// Adds the provided <paramref name="event"/> to the <see cref="QueueTracker"/> of the queue <paramref name="targetQueueName"/>.
		/// </summary>
		private void EnqueueEvent(string targetQueueName, IEvent<TAuthenticationToken> @event)
		{
			var queue = QueueTracker.GetOrAdd(targetQueueName, new ConcurrentQueue<IEvent<TAuthenticationToken>>());
			queue.Enqueue(@event);
		}

		/// <summary>
		/// Creates the queue of the name <paramref name="queueName"/> if it does not already exist,
		/// the queue is attached to <see cref="DequeuAndProcessEvent"/> using a <see cref="Thread"/>.
		/// </summary>
		/// <param name="queueName">The name of the queue to check and create.</param>
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

		/// <summary>
		/// Takes an <see cref="IEvent{TAuthenticationToken}"/> off the queue of <paramref name="queueName"/>
		/// and calls <see cref="ReceiveEvent"/>. Repeats in a loop until the queue is empty.
		/// </summary>
		/// <param name="queueName">The name of the queue process.</param>
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

		/// <summary>
		/// The number of queues currently known.
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
		/// The name of all currently known queues.
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
	}
}