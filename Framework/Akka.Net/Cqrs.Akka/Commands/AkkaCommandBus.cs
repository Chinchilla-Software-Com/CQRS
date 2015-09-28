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
		: ICommandSender<TAuthenticationToken>
	{
		internal AkkaCommandBus(IActorRef actorReference, ICommandSender<TAuthenticationToken> commandSender)
		{
			ActorReference = actorReference;
			CommandSender = commandSender;
		}

		protected IActorRef ActorReference { get; private set; }

		protected ICommandSender<TAuthenticationToken> CommandSender { get; private set; }

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			ActorReference.Tell(command);

			command.Framework = FrameworkType.Akka;
			CommandSender.Send(command);
		}

		#endregion
	}
}