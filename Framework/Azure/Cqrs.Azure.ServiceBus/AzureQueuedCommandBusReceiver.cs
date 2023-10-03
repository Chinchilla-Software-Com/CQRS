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
using Cqrs.Commands;
using Cqrs.Configuration;
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
#else
using Microsoft.ServiceBus.Messaging;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif
using SpinWait = Cqrs.Infrastructure.SpinWait;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A concurrent implementation of <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/> that resides in memory.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureQueuedCommandBusReceiver<TAuthenticationToken> : AzureCommandBusReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// Tracks all queues.
		/// </summary>
		protected static ConcurrentDictionary<string, ConcurrentQueue<ICommand<TAuthenticationToken>>> QueueTracker { get; private set; }

		/// <summary>
		/// Gets the <see cref="ReaderWriterLockSlim"/>.
		/// </summary>
		protected ReaderWriterLockSlim QueueTrackerLock { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureQueuedCommandBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureQueuedCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory)
		{
			QueueTracker = new ConcurrentDictionary<string, ConcurrentQueue<ICommand<TAuthenticationToken>>>();
			QueueTrackerLock = new ReaderWriterLockSlim();
		}

		/// <summary>
		/// Receives a <see cref="BrokeredMessage"/> from the command bus, identifies a key and queues it accordingly.
		/// </summary>
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		protected override void ReceiveCommand(IMessageReceiver client, BrokeredMessage message)
#else
		protected override void ReceiveCommand(IMessageReceiver serviceBusReceiver, BrokeredMessage message)
#endif
		{
			try
			{
				Logger.LogDebug(string.Format("A command message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBodyAsString();
				ICommand<TAuthenticationToken> command = MessageSerialiser.DeserialiseCommand(messageBody);

				CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
				string topicPath = client == null ? "UNKNOWN" : client.Path;
#else
				string topicPath = serviceBusReceiver == null ? "UNKNOWN" : serviceBusReceiver.TopicPath;
#endif
				Logger.LogInfo($"A command message arrived from topic {topicPath} with the {message.MessageId} was of type {command.GetType().FullName}.");

				Type commandType = command.GetType();

				string targetQueueName = commandType.FullName;

				try
				{
					object rsn = commandType.GetProperty("Rsn").GetValue(command, null);
					targetQueueName = string.Format("{0}.{1}", targetQueueName, rsn);
				}
				catch
				{
					Logger.LogDebug(string.Format("A command message arrived with the id '{0}' was of type {1} but with no Rsn property.", message.MessageId, commandType));
					// Do nothing if there is no rsn. Just use command type name
				}

				CreateQueueAndAttachListenerIfNotExist(targetQueueName);
				EnqueueCommand(targetQueueName, command);

				// remove the original message from the incoming queue
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
				client.CompleteAsync(message.SystemProperties.LockToken).Wait(1500);
#else
				message.Complete();
#endif

				Logger.LogDebug(string.Format("A command message arrived and was processed with the id '{0}'.", message.MessageId));
			}
			catch (Exception exception)
			{
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
				client.AbandonAsync(message.SystemProperties.LockToken).Wait(1500);
#else
				message.Abandon();
#endif
			}
		}

		/// <summary>
		/// Adds the provided <paramref name="command"/> to the <see cref="QueueTracker"/> of the queue <paramref name="targetQueueName"/>.
		/// </summary>
		private void EnqueueCommand(string targetQueueName, ICommand<TAuthenticationToken> command)
		{
			var queue = QueueTracker.GetOrAdd(targetQueueName, new ConcurrentQueue<ICommand<TAuthenticationToken>>());
			queue.Enqueue(command);
		}

		/// <summary>
		/// Creates the queue of the name <paramref name="queueName"/> if it does not already exist,
		/// the queue is attached to <see cref="DequeuAndProcessCommand"/> using a <see cref="Thread"/>.
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
		/// Takes an <see cref="ICommand{TAuthenticationToken}"/> off the queue of <paramref name="queueName"/>
		/// and calls <see cref="ReceiveCommand"/>. Repeats in a loop until the queue is empty.
		/// </summary>
		/// <param name="queueName">The name of the queue process.</param>
		protected void DequeuAndProcessCommand(string queueName)
		{
			SpinWait.SpinUntil
			(
				() =>
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