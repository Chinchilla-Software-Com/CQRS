using System;
using Akka.Actor;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Aggregates;
using Cqrs.Commands;

namespace Cqrs.Akka.Tests.Unit.Commands.Handlers
{
	public class ReplyToHelloWorldCommandHandler
		: ICommandHandler<Guid, ReplyToHelloWorldCommand>
	{
		/// <summary>
		/// Instantiates the <see cref="ReplyToHelloWorldCommandHandler"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public ReplyToHelloWorldCommandHandler(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in ReplyToHelloWorldCommand>

		public void Handle(ReplyToHelloWorldCommand command)
		{
			IActorRef item = AggregateResolver.ResolveActor<HelloWorld, Guid>(command.Id);
			bool result = item.Ask<bool>(command).Result;
			// item.Tell(parameters);
		}

		#endregion
	}
}