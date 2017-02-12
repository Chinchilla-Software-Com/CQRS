using System;
using Akka.Actor;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Aggregates;
using Cqrs.Commands;

namespace Cqrs.Akka.Tests.Unit.Commands.Handlers
{
	public class EndConversationCommandHandler
		: ICommandHandler<Guid, EndConversationCommand>
	{
		/// <summary>
		/// Instantiates the <see cref="EndConversationCommandHandler"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public EndConversationCommandHandler(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in EndConversationCommand>

		public void Handle(EndConversationCommand command)
		{
			IActorRef item = AggregateResolver.ResolveActor<HelloWorld, Guid>(command.Id);
			var parameters = new EndConversationParameters();
			bool result = item.Ask<bool>(parameters).Result;
			// item.Tell(parameters);
		}

		#endregion
	}
}