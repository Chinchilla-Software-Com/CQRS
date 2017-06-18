using Cqrs.Commands;
using Cqrs.Events;
using HelloWorldExample.Controllers.Events;

namespace HelloWorldExample.Controllers.Commands.Handlers
{
	public class SayHelloCommandHandler : ICommandHandler<string, SayHelloCommand>
	{
		public SayHelloCommandHandler(IEventPublisher<string> eventPublisher)
		{
			EventPublisher = eventPublisher;
		}

		protected IEventPublisher<string> EventPublisher { get; private set; }

		#region Implementation of IMessageHandler<in SayHelloCommand>

		public void Handle(SayHelloCommand message)
		{
			EventPublisher.Publish(new HelloSaidEvent());
		}

		#endregion
	}
}