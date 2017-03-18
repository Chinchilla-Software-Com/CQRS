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
using System.Text;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using cdmdotnet.Logging;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusPublisher<TAuthenticationToken> : AzureEventHubBus<TAuthenticationToken>, IEventPublisher<TAuthenticationToken>
	{
		public AzureEventBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, true)
		{
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			if (!AzureBusHelper.PrepareAndValidateEvent(@event, "Azure-EventHub"))
				return;

			try
			{
				var brokeredMessage = new Microsoft.ServiceBus.Messaging.EventData(Encoding.UTF8.GetBytes(MessageSerialiser.SerialiseEvent(@event)));
				brokeredMessage.Properties.Add("Type", @event.GetType().FullName);

				EventHubPublisher.Send(brokeredMessage);
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
			Logger.LogInfo(string.Format("An event was published with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
		}

		public virtual void Publish<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<TAuthenticationToken>
		{
			IList<TEvent> sourceEvents = events.ToList();
			IList<string> sourceEventMessages = new List<string>();
			IList<Microsoft.ServiceBus.Messaging.EventData> brokeredMessages = new List<Microsoft.ServiceBus.Messaging.EventData>(sourceEvents.Count);
			foreach (TEvent @event in sourceEvents)
			{
				if (!AzureBusHelper.PrepareAndValidateEvent(@event, "Azure-EventHub"))
					continue;

				var brokeredMessage = new Microsoft.ServiceBus.Messaging.EventData(Encoding.UTF8.GetBytes(MessageSerialiser.SerialiseEvent(@event)));
				brokeredMessage.Properties.Add("Type", @event.GetType().FullName);

				brokeredMessages.Add(brokeredMessage);
				sourceEventMessages.Add(string.Format("A command was sent of type {0}.", @event.GetType().FullName));
			}

			try
			{
				EventHubPublisher.SendBatch(brokeredMessages);
			}
			catch (QuotaExceededException exception)
			{
				Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Events", sourceEvents } });
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogError("An issue occurred while trying to publish a event.", exception: exception, metaData: new Dictionary<string, object> { { "Events", sourceEvents } });
				throw;
			}

			foreach (string message in sourceEventMessages)
				Logger.LogInfo(message);
		}

		#endregion
	}
}
