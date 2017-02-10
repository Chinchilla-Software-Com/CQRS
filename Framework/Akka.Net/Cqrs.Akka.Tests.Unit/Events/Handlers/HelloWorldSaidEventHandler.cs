using System;
using Cqrs.Akka.Tests.Unit.Commands;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	public class HelloWorldSaidEventHandler
		: IEventHandler<Guid, HelloWorldSaid>
	{
		public HelloWorldSaidEventHandler(ICommandSender<Guid> commandBus)
		{
			CommandBus = commandBus;
		}

		protected ICommandSender<Guid> CommandBus { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldSaid>

		public void Handle(HelloWorldSaid message)
		{
			CommandBus.Send(new HelloWorldReplyCommand {Id = message.Id});
		}

		#endregion
	}
}