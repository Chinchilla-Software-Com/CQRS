#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;
using EventData = Microsoft.ServiceBus.Messaging.EventData;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusReceiver<TAuthenticationToken>
		: AzureEventHubBus<TAuthenticationToken>
		, IEventHandlerRegistrar
		, IEventReceiver<TAuthenticationToken>
	{
		protected virtual string NumberOfReceiversCountConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.NumberOfReceiversCount"; }
		}

		// ReSharper disable StaticMemberInGenericType
		protected static RouteManager Routes { get; private set; }
		// ReSharper restore StaticMemberInGenericType

		protected ITelemetryHelper TelemetryHelper { get; private set; }

		static AzureEventBusReceiver()
		{
			Routes = new RouteManager();
		}

		public AzureEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.EventHub.EventBus.Receiver.UseApplicationInsightTelemetryHelper");
		}

		public void Start()
		{
			InstantiateReceiving();

			// Callback to handle received messages
			RegisterReceiverMessageHandler(ReceiveEvent);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			AzureBusHelper.RegisterHandler(TelemetryHelper, Routes, handler, targetedType, holdMessageLock);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = false)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		protected virtual void ReceiveEvent(PartitionContext context, EventData eventData)
		{
			// Do a manual 10 try attempt with back-off
			for (int i = 0; i < 10; i++)
			{
				try
				{
					Logger.LogDebug(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}'.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset));
					string messageBody = Encoding.UTF8.GetString(eventData.GetBytes());

					IEvent<TAuthenticationToken> @event = AzureBusHelper.ReceiveEvent(messageBody, ReceiveEvent,
						string.Format("partition key '{0}', sequence number '{1}' and offset '{2}'", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset),
						() =>
						{
							// Remove message from queue
							context.CheckpointAsync(eventData);
							Logger.LogDebug(string.Format("An event message arrived with the partition key '{0}', sequence number '{1}' and offset '{2}' but processing was skipped due to event settings.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset));
						}
					);

					// Remove message from queue
					context.CheckpointAsync(eventData);
					Logger.LogDebug(string.Format("An event message arrived and was processed with the partition key '{0}', sequence number '{1}' and offset '{2}'.", eventData.PartitionKey, eventData.SequenceNumber, eventData.Offset));

					IList<IEvent<TAuthenticationToken>> events;
					if (EventWaits.TryGetValue(@event.CorrelationId, out events))
						events.Add(@event);

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

		public virtual void ReceiveEvent(IEvent<TAuthenticationToken> @event)
		{
			AzureBusHelper.DefaultReceiveEvent(@event, Routes, "Azure-EventHub");
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		protected override int GetCurrentNumberOfReceiversCount()
		{
			string numberOfReceiversCountValue;
			int numberOfReceiversCount;
			if (ConfigurationManager.TryGetSetting(NumberOfReceiversCountConfigurationKey, out numberOfReceiversCountValue) && !string.IsNullOrWhiteSpace(numberOfReceiversCountValue))
			{
				if (!int.TryParse(numberOfReceiversCountValue, out numberOfReceiversCount))
					numberOfReceiversCount = DefaultNumberOfReceiversCount;
			}
			else
				numberOfReceiversCount = DefaultNumberOfReceiversCount;
			return numberOfReceiversCount;
		}

		#endregion
	}
}