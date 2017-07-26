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
	/// <summary>
	/// Handles the <see cref="HelloWorldSaid"/>.
	/// </summary>
	public class HelloWorldSaidEventHandler
		: IEventHandler<Guid, HelloWorldSaid>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="HelloWorldSaidEventHandler"/>.
		/// </summary>
		public HelloWorldSaidEventHandler(IAkkaCommandPublisher<Guid> commandBus)
		{
			CommandBus = commandBus;
		}

		/// <summary>
		/// Publish any <see cref="ICommand{TAuthenticationToken}"/> instances that you want to send with this.
		/// </summary>
		protected ICommandPublisher<Guid> CommandBus { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldSaid>

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="ReplyToHelloWorldCommand"/> to respond to or "handle"</param>
		public void Handle(HelloWorldSaid message)
		{
			CommandBus.Publish(new ReplyToHelloWorldCommand {Id = message.Id});
			AkkaUnitTests.Step1Reached[message.CorrelationId] = true;
		}

		#endregion
	}
}