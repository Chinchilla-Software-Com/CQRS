#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Cqrs.Commands;
using Cqrs.Messages;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="ICommandSender{TAuthenticationToken}"/> that proxies <see cref="ICommand{TAuthenticationToken}"/> back onto the <see cref="IActorRef"/> and then publishes the <see cref="ICommand{TAuthenticationToken}"/> on the public command bus.
	/// </summary>
	public class AkkaCommandBus<TAuthenticationToken>
		: IAkkaCommandSender<TAuthenticationToken>
	{
		protected IActorRef ActorReference { get; private set; }

		protected ICommandSender<TAuthenticationToken> CommandSender { get; private set; }

		protected ICommandReceiver<TAuthenticationToken> CommandReceiver { get; private set; }

		internal AkkaCommandBus(IActorRef actorReference, ICommandSender<TAuthenticationToken> commandSender, ICommandReceiver<TAuthenticationToken> commandReceiver)
		{
			ActorReference = actorReference;
			CommandSender = commandSender;
			CommandReceiver = commandReceiver;
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			// This will trigger the Akka cycle back publishing... It looks weird, but trust it
			CommandReceiver.ReceiveCommand(command);

			command.Framework = FrameworkType.Akka;
			CommandSender.Send(command);
		}

		#endregion
	}
}