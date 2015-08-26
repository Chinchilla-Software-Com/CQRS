using System;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using cdmdotnet.Logging;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusPublisher<TAuthenticationToken> : AzureEventBus<TAuthenticationToken>, IEventPublisher<TAuthenticationToken>
	{
		public AzureEventBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper CorrelationIdHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, CorrelationIdHelper)
		{
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			if (@event.AuthenticationToken == null)
				@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();
			@event.TimeStamp = DateTimeOffset.UtcNow;

			ServiceBusPublisher.Send(new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event)));
		}

		#endregion
	}
}
