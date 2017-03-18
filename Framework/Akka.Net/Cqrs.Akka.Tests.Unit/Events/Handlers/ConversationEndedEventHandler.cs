using System;
using Cqrs.Akka.Commands;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	public class ConversationEndedEventHandler
		: IEventHandler<Guid, ConversationEnded>
	{
		public ConversationEndedEventHandler(IAkkaCommandSender<Guid> commandBus)
		{
			CommandBus = commandBus;
		}

		protected ICommandPublisher<Guid> CommandBus { get; private set; }

		#region Implementation of IMessageHandler<in ConversationEnded>

		public void Handle(ConversationEnded message)
		{
			AkkaUnitTests.Step4Reached[message.CorrelationId] = true;
		}

		#endregion
	}
}