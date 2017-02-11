using System;
using Akka.Actor;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Aggregates;
using Cqrs.Commands;

namespace Cqrs.Akka.Tests.Unit.Commands.Handlers
{
	public class SayHelloWorldCommandHandler
		: ICommandHandler<Guid, SayHelloWorldCommand>
	{
		/// <summary>
		/// Instantiates the <see cref="SayHelloWorldCommandHandler"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public SayHelloWorldCommandHandler(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in SayHelloWorldCommand>

		public void Handle(SayHelloWorldCommand command)
		{
			if (command.Id == Guid.Empty)
				command.Id = Guid.NewGuid();
			IActorRef item = AggregateResolver.ResolveActor<HelloWorld, Guid>(command.Id);
			var parameters = new SayHelloParameters();
			bool result = item.Ask<bool>(parameters).Result;
			// item.Tell(parameters);
		}

		#endregion
	}
}