using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureMessageBusPublisher<TAuthenticationToken> : AzureMessageBus<TAuthenticationToken>, IEventPublisher<TAuthenticationToken>
	{
		public AzureMessageBusPublisher(IConfigurationManager configurationManager, IEventSerialiser<TAuthenticationToken> eventSerialiser)
			: base(configurationManager, eventSerialiser)
		{
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			ServiceBusClient.Send(new BrokeredMessage(EventSerialiser.SerialisEvent(@event)));
		}

		#endregion
	}
}
