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
using System.Threading;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
#if NET452
using Microsoft.ServiceBus.Messaging;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif
#if NETSTANDARD2_0
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
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
		/// Instantiates a new instance of <see cref="AzureQueuedEventBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureQueuedEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory)
		{
			QueueTracker = new ConcurrentDictionary<string, ConcurrentQueue<IEvent<TAuthenticationToken>>>();
			QueueTrackerLock = new ReaderWriterLockSlim();
		}

		/// <summary>
		/// Receives a <see cref="BrokeredMessage"/> from the event bus, identifies a key and queues it accordingly.
		/// </summary>
#if NET452
		protected override void ReceiveEvent(IMessageReceiver serviceBusReceiver, BrokeredMessage message)
#endif
#if NETSTANDARD2_0
		protected override void ReceiveEvent(IMessageReceiver client, BrokeredMessage message)
#endif
		{
			try
			{
				Logger.LogDebug(string.Format("An event message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBodyAsString();
				IEvent<TAuthenticationToken> @event = MessageSerialiser.DeserialiseEvent(messageBody);

				CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
#if NET452
				string topicPath = serviceBusReceiver == null ? "UNKNOWN" : serviceBusReceiver.TopicPath;
#endif
#if NETSTANDARD2_0
				string topicPath = client == null ? "UNKNOWN" : client.Path;
#endif
				Logger.LogInfo($"An event message arrived from topic {topicPath} with the {message.MessageId} was of type {@event.GetType().FullName}.");

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
#if NET452
				message.Complete();
#endif
#if NETSTANDARD2_0
				client.CompleteAsync(message.SystemProperties.LockToken).Wait(1500);
#endif

				Logger.LogDebug(string.Format("An event message arrived and was processed with the id '{0}'.", message.MessageId));
			}
			catch (Exception exception)
			{
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("An event message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
#if NET452
				message.Abandon();
#endif
#if NETSTANDARD2_0
				client.AbandonAsync(message.SystemProperties.LockToken).Wait(1500);
#endif
			}
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