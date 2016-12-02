#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusPublisher<TAuthenticationToken> : AzureEventBus<TAuthenticationToken>, IEventPublisher<TAuthenticationToken>
	{
		public AzureEventBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, true)
		{
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
				PublicServiceBusPublisher.Send(new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event)));
				Logger.LogDebug(string.Format("An event was published on the public bus with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
			}
			if (publicEventAttribute != null)
			{
				PublicServiceBusPublisher.Send(new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event)));
				Logger.LogDebug(string.Format("An event was published on the public bus with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
			}
			if (privateEventAttribute != null)
			{
				PrivateServiceBusPublisher.Send(new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event)));
				Logger.LogDebug(string.Format("An event was published on the private bus with the id '{0}' was of type {1}.", @event.Id, @event.GetType().FullName));
			}
		}

		#endregion
	}
}
