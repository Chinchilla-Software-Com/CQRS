#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Tests.Unit.Aggregates;
using Cqrs.Commands;
using Cqrs.Domain;

namespace Cqrs.Akka.Tests.Unit.Commands.Handlers
{
	/// <summary>
	/// Handles the <see cref="SayHelloWorldCommand"/>.
	/// </summary>
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

		/// <summary>
		/// Resolves Akka.Net actor based <see cref="IAggregateRoot{TAuthenticationToken}"/>
		/// </summary>
		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in SayHelloWorldCommand>

		/// <summary>
		/// Responds to the provided <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The <see cref="SayHelloWorldCommand"/> to respond to or "handle"</param>
		public void Handle(SayHelloWorldCommand command)
		{
			if (command.Id == Guid.Empty)
				command.Id = Guid.NewGuid();
			IActorRef item = AggregateResolver.ResolveActor<HelloWorld, Guid>(command.Id);
			bool result = item.Ask<bool>(command).Result;
			// item.Tell(parameters);
		}

		#endregion
	}
}