using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using cdmdotnet.Logging;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusPublisher<TAuthenticationToken> : AzureEventBus<TAuthenticationToken>, IEventPublisher<TAuthenticationToken>
	{
		public AzureEventBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrolationIdHelper corrolationIdHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, corrolationIdHelper)
		{
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			if (@event.AuthenticationToken == null)
				@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			@event.CorrelationId = CorrolationIdHelper.GetCorrolationId();

			ServiceBusClient.Send(new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event)));
		}

		#endregion
	}
}
