#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chinchilla.Logging;
#if NET452
using Microsoft.ServiceBus.Messaging;
#endif
#if NETSTANDARD2_0
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A default implementation of <see cref="IEventProcessor"/> suitable for most situations and conditions.
	/// </summary>
	internal class DefaultEventProcessor : IEventProcessor
	{
		protected ILogger Logger { get; private set; }

		protected Action<PartitionContext, EventData> ReceiverMessageHandler { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultEventProcessor"/> class.
		/// </summary>
		public DefaultEventProcessor(ILogger logger, Action<PartitionContext, EventData> receiverMessageHandler)
		{
			Logger = logger;
			ReceiverMessageHandler = receiverMessageHandler;
		}

		#region Implementation of IEventProcessor

		/// <summary>
		/// Initializes the Event Hub processor instance. This method is called before any event data is passed to this processor instance.
		/// </summary>
		/// <param name="context">Ownership information for the partition on which this processor instance works. Any attempt to call <see cref="M:Microsoft.ServiceBus.Messaging.PartitionContext.CheckpointAsync"/> will fail during the Open operation.</param>
		/// <returns>
		/// The task that indicates that the Open operation is complete.
		/// </returns>
		public virtual Task OpenAsync(PartitionContext context)
		{
			Logger.LogInfo("Open Async");
			return Task.FromResult<object>(null);
		}

		/// <summary>
		/// Asynchronously processes the specified context and messages. This method is called when there are new messages in the Event Hubs stream. Make sure to checkpoint only when you are finished processing all the events in each batch.
		/// </summary>
		/// <param name="context">Ownership information for the partition on which this processor instance works.</param>
		/// <param name="messages">A batch of Event Hubs events.</param>
		/// <returns>
		/// The task that indicates that <see cref="M:Microsoft.ServiceBus.Messaging.IEventProcessor.ProcessEventsAsync(Microsoft.ServiceBus.Messaging.PartitionContext,System.Collections.Generic.IEnumerable{Microsoft.ServiceBus.Messaging.EventData})"/> is complete.
		/// </returns>
		public virtual Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
		{
			Task results = new TaskFactory().StartNew(() =>
			{
				foreach (EventData eventData in messages)
					ReceiverMessageHandler(context, eventData);
			});

			return results;
		}

		/// <summary>
		/// Called when the ownership of partition moves to a different node for load-balancing purpose, or when the host is shutting down. Called in response to <see cref="M:Microsoft.ServiceBus.Messaging.EventHubConsumerGroup.UnregisterProcessorAsync(Microsoft.ServiceBus.Messaging.Lease,Microsoft.ServiceBus.Messaging.CloseReason)"/>.
		/// </summary>
		/// <param name="context">Partition ownership information for the partition on which this processor instance works. You can call <see cref="M:Microsoft.ServiceBus.Messaging.PartitionContext.CheckpointAsync"/> to checkpoint progress in the processing of messages from Event Hub streams.</param>
		/// <param name="reason">The reason for calling <see cref="M:Microsoft.ServiceBus.Messaging.IEventProcessor.CloseAsync(Microsoft.ServiceBus.Messaging.PartitionContext,Microsoft.ServiceBus.Messaging.CloseReason)"/>.</param>
		/// <returns>
		/// A task indicating that the Close operation is complete.
		/// </returns>
		public virtual Task CloseAsync(PartitionContext context, CloseReason reason)
		{
			Logger.LogInfo("Close Async");
			return Task.FromResult<object>(null);
		}

		public Task ProcessErrorAsync(PartitionContext context, Exception error)
		{
			Logger.LogInfo("Process Error Async");
			return Task.FromResult<object>(null);
		}

		#endregion
	}
}