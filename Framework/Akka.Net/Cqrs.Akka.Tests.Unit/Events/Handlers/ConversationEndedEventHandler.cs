#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Akka.Commands;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Akka.Tests.Unit.Events.Handlers
{
	/// <summary>
	/// Handles the <see cref="ConversationEnded"/>.
	/// </summary>
	public class ConversationEndedEventHandler
		: IEventHandler<Guid, ConversationEnded>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="ConversationEndedEventHandler"/>.
		/// </summary>
		public ConversationEndedEventHandler(IAkkaCommandPublisher<Guid> commandBus)
		{
			CommandBus = commandBus;
		}

		/// <summary>
		/// Publish any <see cref="ICommand{TAuthenticationToken}"/> instances that you want to send with this.
		/// </summary>
		protected ICommandPublisher<Guid> CommandBus { get; private set; }

		#region Implementation of IMessageHandler<in ConversationEnded>

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="ConversationEnded"/> to respond to or "handle"</param>
		public void Handle(ConversationEnded message)
		{
			AkkaUnitTests.Step4Reached[message.CorrelationId] = true;
		}

		#endregion
	}
}