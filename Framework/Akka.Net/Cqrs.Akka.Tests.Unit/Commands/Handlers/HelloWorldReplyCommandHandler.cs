using System;
using Akka.Actor;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Aggregates;
using Cqrs.Commands;

namespace Cqrs.Akka.Tests.Unit.Commands.Handlers
{
	public class HelloWorldReplyCommandHandler
		: ICommandHandler<Guid, HelloWorldReplyCommand>
	{
		/// <summary>
		/// Instantiates the <see cref="SayHelloWorldCommandHandler"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public HelloWorldReplyCommandHandler(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in HelloWorldReplyCommand>

		public void Handle(HelloWorldReplyCommand command)
		{
			IActorRef item = AggregateResolver.Resolve<HelloWorld>(command.Id);
			var parameters = new HelloWorldReplyParameters();
			bool result = item.Ask<bool>(parameters).Result;
			// item.Tell(parameters);
		}

		#endregion
	}
}