#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Akka.Commands;
using Cqrs.Akka.Tests.Unit.Commands;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	public class HelloWorldSaidEventHandler
		: IEventHandler<Guid, HelloWorldSaid>
	{
		public HelloWorldSaidEventHandler(IAkkaCommandPublisher<Guid> commandBus)
		{
			CommandBus = commandBus;
		}

		protected ICommandPublisher<Guid> CommandBus { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldSaid>

		public void Handle(HelloWorldSaid message)
		{
			CommandBus.Publish(new ReplyToHelloWorldCommand {Id = message.Id});
			AkkaUnitTests.Step1Reached[message.CorrelationId] = true;
		}

		#endregion
	}
}