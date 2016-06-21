#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	public class InProcessBus<TAuthenticationToken>
		: ICommandSender<TAuthenticationToken>
		, IEventPublisher<TAuthenticationToken>
		, IEventHandlerRegistrar
		, ICommandHandlerRegistrar
	{
		private static RouteManager Routes { get; set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		static InProcessBus()
		{
			Routes = new RouteManager();
		}

		public InProcessBus(IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, IDependencyResolver dependencyResolver, ILogger logger, IConfigurationManager configurationManager)
		{
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			DependencyResolver = dependencyResolver;
			Logger = logger;
			ConfigurationManager = configurationManager;
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public virtual void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Type commandType = command.GetType();
			switch (command.Framework)
			{
				case FrameworkType.Akka:
					Logger.LogInfo(string.Format("A command arrived of the type '{0}' but was marked as coming from the '{1}' framework, so it was dropped.", commandType.FullName, command.Framework));
					return;
			}

			ICommandValidator<TAuthenticationToken, TCommand> commandValidator = null;
			try
			{
				commandValidator = DependencyResolver.Resolve<ICommandValidator<TAuthenticationToken, TCommand>>();
			}
			catch (Exception exception)
			{
				Logger.LogDebug("Locating an ICommandValidator failed.", string.Format("{0}\\Handle({1})", GetType().FullName, commandType.FullName), exception);
			}

			if (commandValidator != null && !commandValidator.IsCommandValid(command))
			{
				Logger.LogInfo("The provided command is not valid.", string.Format("{0}\\Handle({1})", GetType().FullName, commandType.FullName));
				return;
			}

			if (command.AuthenticationToken == null)
				command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			bool isRequired;
			if (!ConfigurationManager.TryGetSetting(string.Format("{0}.IsRequired", commandType.FullName), out isRequired))
				isRequired = true;

			RouteHandlerDelegate commandHandler = Routes.GetSingleHandler(command, isRequired);
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (commandHandler == null)
			{
				Logger.LogDebug(string.Format("The command handler for '{0}' is not required.", commandType.FullName));
				return;
			}

			Action<IMessage> handler = commandHandler.Delegate;
			handler(command);
		}

		#endregion

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public virtual void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			switch (@event.Framework)
			{
				case FrameworkType.Akka:
					Logger.LogInfo(string.Format("An event arrived of the type '{0}' but was marked as coming from the '{1}' framework, so it was dropped.", @event.GetType().FullName, @event.Framework));
					return;
			}

			if (@event.AuthenticationToken == null)
				@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			@event.CorrelationId = CorrelationIdHelper.GetCorrelationId();
			@event.TimeStamp = DateTimeOffset.UtcNow;

			Type eventType = @event.GetType();
			bool isRequired;
			if (!ConfigurationManager.TryGetSetting(string.Format("{0}.IsRequired", eventType.FullName), out isRequired))
				isRequired = true;

			IEnumerable<Action<IMessage>> handlers = Routes.GetHandlers(@event, isRequired).Select(x => x.Delegate);
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (!handlers.Any())
				Logger.LogDebug(string.Format("The event handler for '{0}' is not required.", eventType.FullName));

			foreach (Action<IMessage> handler in handlers)
				handler(@event);
		}

		#endregion

		#region Implementation of IHandlerRegistrar

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType)
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