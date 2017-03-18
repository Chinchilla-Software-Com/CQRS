#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusPublisher<TAuthenticationToken> : AzureEventBus<TAuthenticationToken>, IEventPublisher<TAuthenticationToken>
	{
		public AzureEventBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, BusHelper busHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, true)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.EventBus.Publisher.UseApplicationInsightTelemetryHelper", correlationIdHelper);
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			if (!AzureBusHelper.PrepareAndValidateEvent(@event, "Azure-ServiceBus"))
				return;

			var privateEventAttribute = Attribute.GetCustomAttribute(typeof(TEvent), typeof(PrivateEventAttribute)) as PrivateEventAttribute;
			var publicEventAttribute = Attribute.GetCustomAttribute(typeof(TEvent), typeof(PrivateEventAttribute)) as PublicEventAttribute;

			// Backwards compatibility and simplicity
			if (publicEventAttribute == null && privateEventAttribute == null)
			{
				try
				{
					var brokeredMessage = new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event))
					{
						CorrelationId = CorrelationIdHelper.GetCorrelationId().ToString("N")
					};
					brokeredMessage.Properties.Add("Type", @event.GetType().FullName);
					PublicServiceBusPublisher.Send(brokeredMessage);
				}
				catch (QuotaExceededException exception)
				{
					Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Event", @event } });
					throw;
				}
				catch (Exception exception)
				{
					Logger.LogError("An issue occurred while trying to publish an event.", exception: exception, metaData: new Dictionary<string, object> { { "Event", @event } });
					throw;
				}
				Logger.LogDebug(string.Format("An event was published on the public bus with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
			}
			if (publicEventAttribute != null)
			{
				try
				{
					var brokeredMessage = new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event))
					{
						CorrelationId = CorrelationIdHelper.GetCorrelationId().ToString("N")
					};
					brokeredMessage.Properties.Add("Type", @event.GetType().FullName);
					PublicServiceBusPublisher.Send(brokeredMessage);
				}
				catch (QuotaExceededException exception)
				{
					Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Event", @event } });
					throw;
				}
				catch (Exception exception)
				{
					Logger.LogError("An issue occurred while trying to publish an event.", exception: exception, metaData: new Dictionary<string, object> { { "Event", @event } });
					throw;
				}
				Logger.LogDebug(string.Format("An event was published on the public bus with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
			}
			if (privateEventAttribute != null)
			{
				try
				{
					var brokeredMessage = new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event))
					{
						CorrelationId = CorrelationIdHelper.GetCorrelationId().ToString("N")
					};
					brokeredMessage.Properties.Add("Type", @event.GetType().FullName);
					PrivateServiceBusPublisher.Send(brokeredMessage);
				}
				catch (QuotaExceededException exception)
				{
					Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Event", @event } });
					throw;
				}
				catch (Exception exception)
				{
					Logger.LogError("An issue occurred while trying to publish an event.", exception: exception, metaData: new Dictionary<string, object> { { "Event", @event } });
					throw;
				}
				Logger.LogDebug(string.Format("An event was published on the private bus with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
			}
		}

		public virtual void Publish<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<TAuthenticationToken>
		{
			IList<TEvent> sourceEvents = events.ToList();
			IList<string> sourceEventMessages = new List<string>();
			IList<BrokeredMessage> privateBrokeredMessages = new List<BrokeredMessage>(sourceEvents.Count);
			IList<BrokeredMessage> publicBrokeredMessages = new List<BrokeredMessage>(sourceEvents.Count);
			foreach (TEvent @event in sourceEvents)
			{
				if (!AzureBusHelper.PrepareAndValidateEvent(@event, "Azure-ServiceBus"))
					continue;

				var brokeredMessage = new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event))
				{
					CorrelationId = CorrelationIdHelper.GetCorrelationId().ToString("N")
				};
				brokeredMessage.Properties.Add("Type", @event.GetType().FullName);

				var privateEventAttribute = Attribute.GetCustomAttribute(typeof(TEvent), typeof(PrivateEventAttribute)) as PrivateEventAttribute;
				var publicEventAttribute = Attribute.GetCustomAttribute(typeof(TEvent), typeof(PrivateEventAttribute)) as PublicEventAttribute;

				if
					(
					// Backwards compatibility and simplicity
					(publicEventAttribute == null && privateEventAttribute == null)
					||
					publicEventAttribute != null
					)
				{
					publicBrokeredMessages.Add(brokeredMessage);
					sourceEventMessages.Add(string.Format("An event was published on the public bus with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
				}
				if (privateEventAttribute != null)
				{
					privateBrokeredMessages.Add(brokeredMessage);
					sourceEventMessages.Add(string.Format("An event was published on the private bus with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
				}
			}

			// Backwards compatibility and simplicity
			try
			{
				PublicServiceBusPublisher.SendBatch(publicBrokeredMessages);
			}
			catch (QuotaExceededException exception)
			{
				Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Events", publicBrokeredMessages } });
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogError("An issue occurred while trying to publish an event.", exception: exception, metaData: new Dictionary<string, object> { { "Events", publicBrokeredMessages } });
				throw;
			}

			try
			{
				PrivateServiceBusPublisher.SendBatch(privateBrokeredMessages);
			}
			catch (QuotaExceededException exception)
			{
				Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Events", privateBrokeredMessages } });
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogError("An issue occurred while trying to publish an event.", exception: exception, metaData: new Dictionary<string, object> { { "Events", privateBrokeredMessages } });
				throw;
			}

			foreach (string message in sourceEventMessages)
				Logger.LogInfo(message);
		}

		#endregion
	}
}
