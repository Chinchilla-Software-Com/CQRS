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
	/// Handles the <see cref="ReplyToHelloWorldCommand"/>.
	/// </summary>
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

		/// <summary>
		/// Resolves Akka.Net actor based <see cref="IAggregateRoot{TAuthenticationToken}"/>
		/// </summary>
		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in ReplyToHelloWorldCommand>

		/// <summary>
		/// Responds to the provided <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The <see cref="ReplyToHelloWorldCommand"/> to respond to or "handle"</param>
		public void Handle(ReplyToHelloWorldCommand command)
		{
			IActorRef item = AggregateResolver.ResolveActor<HelloWorld, Guid>(command.Id);
			bool result = item.Ask<bool>(command).Result;
			// item.Tell(parameters);
		}

		#endregion
	}
}