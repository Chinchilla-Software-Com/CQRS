using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Logging;
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
			@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			@event.CorrolationId = CorrolationIdHelper.GetCorrolationId();

			ServiceBusClient.Send(new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event)));
		}

		#endregion
	}
}
