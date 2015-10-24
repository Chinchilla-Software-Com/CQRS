#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Cqrs.Akka.Configuration;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Messages;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="ICommandSender{TAuthenticationToken}"/> that proxies <see cref="ICommand{TAuthenticationToken}"/> back onto the <see cref="IActorRef"/> and then publishes the <see cref="ICommand{TAuthenticationToken}"/> on the public command bus.
	/// </summary>
	public class AkkaCommandBus<TAuthenticationToken>
		: IAkkaCommandSender<TAuthenticationToken>
		, ICommandHandlerRegistrar
	{
		protected static RouteManager Routes { get; private set; }

		protected IHandlerResolver ConcurrentEventBusProxy { get; private set; }

		static AkkaCommandBus()
		{
			Routes = new RouteManager();
		}

		public AkkaCommandBus(IHandlerResolver concurrentEventBusProxy)
		{
			ConcurrentEventBusProxy = concurrentEventBusProxy;
		}

		protected IActorRef ActorReference { get; private set; }

		protected ICommandSender<TAuthenticationToken> CommandSender { get; private set; }

		protected ICommandReceiver<TAuthenticationToken> CommandReceiver { get; private set; }

		public AkkaCommandBus(IActorRef actorReference, ICommandSender<TAuthenticationToken> commandSender, ICommandReceiver<TAuthenticationToken> commandReceiver)
		{
			ActorReference = actorReference;
			CommandSender = commandSender;
			CommandReceiver = commandReceiver;
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			RouteHandlerDelegate commandHandler = Routes.GetSingleHandler(command);
			Type senderType = typeof (IConcurrentAkkaCommandSender<,>).MakeGenericType(typeof(TAuthenticationToken), commandHandler.TargetedType);
			var proxy = (IActorRef)ConcurrentEventBusProxy.Resolve(senderType, senderType, command.Id);
			proxy.Tell(command);

			command.Framework = FrameworkType.Akka;
			CommandSender.Send(command);
		}

		#endregion

		#region Implementation of IHandlerRegistrar

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType)
			where TMessage : IMessage
		{
			Routes.RegisterHandler(handler, targetedType);
		}

		#endregion
	}
}