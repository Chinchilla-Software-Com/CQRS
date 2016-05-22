#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Cqrs.Akka.Configuration;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Akka.Commands
{
	public class ConcurrentAkkaCommandSender<TAuthenticationToken, TTarget>
		: ReceiveActor
		, IConcurrentAkkaCommandSender<TAuthenticationToken, TTarget>
	{
		protected IActorRef ActorReference { get; private set; }

		protected ICommandReceiver<TAuthenticationToken> CommandReceiver { get; private set; }

		public ConcurrentAkkaCommandSender(IActorRef actorReference, ICommandReceiver<TAuthenticationToken> commandReceiver)
		{
			ActorReference = actorReference;
			CommandReceiver = commandReceiver;
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			// This will trigger the Akka cycle back publishing... It looks weird, but trust it
			CommandReceiver.ReceiveCommand(command);
		}

		#endregion
	}
}