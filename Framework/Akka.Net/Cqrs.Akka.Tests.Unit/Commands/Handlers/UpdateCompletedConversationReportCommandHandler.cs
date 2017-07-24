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

namespace Cqrs.Akka.Tests.Unit.Commands.Handlers
{
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

		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		#region Implementation of IMessageHandler<in UpdateCompletedConversationReportCommand>

		public void Handle(UpdateCompletedConversationReportCommand command)
		{
			AkkaUnitTests.FinalCommandReached[command.CorrelationId] = true;
		}

		#endregion
	}
}