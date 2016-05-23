#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Configuration;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
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

		public AkkaCommandBus(IConfigurationManager configurationManager, ILogger logger, IActorRef actorReference, ICommandSender<TAuthenticationToken> commandSender, ICommandReceiver<TAuthenticationToken> commandReceiver)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
			ActorReference = actorReference;
			CommandSender = commandSender;
			CommandReceiver = commandReceiver;
		}

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected ILogger Logger { get; private set; }

		protected IActorRef ActorReference { get; private set; }

		protected ICommandSender<TAuthenticationToken> CommandSender { get; private set; }

		protected ICommandReceiver<TAuthenticationToken> CommandReceiver { get; private set; }

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Type commandType = typeof(TCommand);

			bool isRequired;
			if (!ConfigurationManager.TryGetSetting(string.Format("{0}.IsRequired", commandType.FullName), out isRequired))
				isRequired = true;

			RouteHandlerDelegate commandHandler = Routes.GetSingleHandler(command, isRequired);
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (commandHandler == null)
				Logger.LogDebug(string.Format("The command handler for '{0}' is not required.", commandType.FullName));

			Type senderType = commandHandler.TargetedType == null
				? typeof (IConcurrentAkkaCommandSender<>).MakeGenericType(typeof(TAuthenticationToken))
				: typeof (IConcurrentAkkaCommandSender<,>).MakeGenericType(typeof(TAuthenticationToken), commandHandler.TargetedType);
			var proxy = (IActorRef)ConcurrentEventBusProxy.Resolve(senderType, command.Id);
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

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null);
		}

		#endregion
	}
}