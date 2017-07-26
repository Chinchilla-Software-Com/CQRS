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
using Cqrs.Commands;
using Cqrs.Domain;

namespace Cqrs.Akka.Tests.Unit.Commands.Handlers
{
	/// <summary>
	/// Handles the <see cref="UpdateCompletedConversationReportCommand"/>.
	/// </summary>
	public class UpdateCompletedConversationReportCommandHandler
		: ICommandHandler<Guid, UpdateCompletedConversationReportCommand>
	{
		/// <summary>
		/// Instantiates the <see cref="SayHelloWorldCommandHandler"/> class registering any <see cref="ReceiveActor.Receive{T}(System.Func{T,System.Threading.Tasks.Task})"/> required.
		/// </summary>
		public UpdateCompletedConversationReportCommandHandler(IAkkaAggregateResolver aggregateResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		/// <summary>
		/// Resolves Akka.Net actor based <see cref="IAggregateRoot{TAuthenticationToken}"/>
		/// </summary>
		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in UpdateCompletedConversationReportCommand>

		/// <summary>
		/// Responds to the provided <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The <see cref="UpdateCompletedConversationReportCommand"/> to respond to or "handle"</param>
		public void Handle(UpdateCompletedConversationReportCommand command)
		{
			AkkaUnitTests.FinalCommandReached[command.CorrelationId] = true;
		}

		#endregion
	}
}