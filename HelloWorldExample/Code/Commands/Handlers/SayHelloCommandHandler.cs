namespace HelloWorld.Code.Commands.Handlers
{
	using Cqrs.Commands;
	using Cqrs.Events;
	using Events;

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