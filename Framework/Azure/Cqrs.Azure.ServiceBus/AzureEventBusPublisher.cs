using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureEventBusPublisher<TAuthenticationToken> : AzureEventBus<TAuthenticationToken>, IEventPublisher<TAuthenticationToken>
	{
		public AzureEventBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper)
		{
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();

			ServiceBusClient.Send(new BrokeredMessage(MessageSerialiser.SerialiseEvent(@event)));
		}

		#endregion
	}
}
