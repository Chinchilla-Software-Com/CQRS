#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Infrastructure;
using Cqrs.Messages;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="ICommandPublisher{TAuthenticationToken}"/> that resolves handlers , executes the handler and then publishes the <see cref="ICommand{TAuthenticationToken}"/> on the public command bus.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AkkaCommandBus<TAuthenticationToken>
		: IAkkaCommandPublisher<TAuthenticationToken>
		, ICommandHandlerRegistrar
	{
		/// <summary>
		/// Gets the <see cref="RouteManager"/>
		/// </summary>
		protected static RouteManager Routes { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}">Authentication Token Helper</see>
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IDependencyResolver"/>
		/// </summary>
		protected IDependencyResolver DependencyResolver { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IBusHelper"/>
		/// </summary>
		protected IBusHelper BusHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ICommandPublisher{TAuthenticationToken}"/>
		/// </summary>
		protected ICommandPublisher<TAuthenticationToken> CommandPublisher { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ICommandReceiver{TAuthenticationToken}"/>
		/// </summary>
		protected ICommandReceiver<TAuthenticationToken> CommandReceiver { get; private set; }

		/// <summary>
		/// Gets or sets the current list of events waiting to be evaluated for <see cref="PublishAndWait{TCommand,TEvent}(TCommand,Cqrs.Events.IEventReceiver{TAuthenticationToken})"/>
		/// </summary>
		protected IDictionary<Guid, IList<IEvent<TAuthenticationToken>>> EventWaits { get; private set; }

		static AkkaCommandBus()
		{
			Routes = new RouteManager();
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaCommandBus{TAuthenticationToken}"/>
		/// </summary>
		public AkkaCommandBus(IBusHelper busHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, IDependencyResolver dependencyResolver, ILogger logger, ICommandPublisher<TAuthenticationToken> commandPublisher, ICommandReceiver<TAuthenticationToken> commandReceiver)
		{
			Logger = logger;
			BusHelper = busHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			DependencyResolver = dependencyResolver;
			EventWaits = new ConcurrentDictionary<Guid, IList<IEvent<TAuthenticationToken>>>();
			CommandPublisher = commandPublisher;
			CommandReceiver = commandReceiver;
		}

		/// <summary>
		/// Sets the
		/// <see cref="IMessageWithAuthenticationToken{TAuthenticationToken}.AuthenticationToken"/>,
		/// <see cref="IMessage.CorrelationId"/>,
		/// <see cref="IMessage.OriginatingFramework"/> to "Akka" and
		/// adds a value of "Akka" to the <see cref="IMessage.Frameworks"/>
		/// if not already done so
		/// </summary>
		protected virtual void PrepareCommand<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			if (command.AuthenticationToken == null || command.AuthenticationToken.Equals(default(TAuthenticationToken)))
				command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			if (string.IsNullOrWhiteSpace(command.OriginatingFramework))
				command.OriginatingFramework = "Akka";

			var frameworks = new List<string>();
			if (command.Frameworks != null)
				frameworks.AddRange(command.Frameworks);
			frameworks.Add("Akka");
			command.Frameworks = frameworks;
		}

		/// <summary>
		/// Locates a suitable <see cref="ICommandValidator{TAuthenticationToken,TCommand}"/> to validate the provided <paramref name="command"/> and validates the provided <paramref name="command"/> if one is located
		/// Calls <see cref="PrepareCommand{TCommand}"/>
		/// Checks if the provided <paramref name="command"/> is required to be processed
		/// Locates a single <see cref="RouteHandlerDelegate">command handler</see> for the provided <paramref name="command"/>
		/// </summary>
		/// <returns>
		/// False if a suitable <see cref="ICommandValidator{TAuthenticationToken,TCommand}"/> is located and the provided <paramref name="command"/> fails validation,
		/// False if no <see cref="RouteHandlerDelegate">command handler</see> is found but the command isn't required to be handled,
		/// True otherwise.
		/// </returns>
		protected virtual bool PrepareAndValidateCommand<TCommand>(TCommand command, out RouteHandlerDelegate commandHandler)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Type commandType = command.GetType();

			if (command.Frameworks != null && command.Frameworks.Contains("Akka"))
			{
				// if this is the only framework in the list, then it's fine to handle as it's just pre-stamped, if there is more than one framework, then exit.
				if (command.Frameworks.Count() != 1)
				{
					Logger.LogInfo("The provided command has already been processed in Akka.", string.Format("{0}\\PrepareAndValidateEvent({1})", GetType().FullName, commandType.FullName));
					commandHandler = null;
					return false;
				}
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
				commandHandler = null;
				return false;
			}

			PrepareCommand(command);

			bool isRequired = BusHelper.IsEventRequired(commandType);

			commandHandler = Routes.GetSingleHandler(command, isRequired);
			// This check doesn't require an isRequired check as there will be an exception raised above and handled below.
			if (commandHandler == null)
				Logger.LogDebug(string.Format("The command handler for '{0}' is not required.", commandType.FullName));

			return true;
		}

		#region Implementation of ICommandPublisher<TAuthenticationToken>

		/// <summary>
		/// Publishes the provided <paramref name="command"/> on the command bus.
		/// </summary>
		public virtual void Publish<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			RouteHandlerDelegate commandHandler;
			if (!PrepareAndValidateCommand(command, out commandHandler))
				return;

			// This could be null if Akka won't handle the command and something else will.
			if (commandHandler != null)
				commandHandler.Delegate(command);

			// Let everything else know about the command (usually double handling a command is bad... but sometimes it might be useful... like pushing from AWS to Azure so both systems handle it... although an event really is the proper pattern to use here.
			CommandPublisher.Publish(command);
		}

		/// <summary>
		/// Publishes the provided <paramref name="commands"/> on the command bus.
		/// </summary>
		public virtual void Publish<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>
		{
			IList<TCommand> sourceCommands = commands.ToList();
			foreach (TCommand command in sourceCommands)
			{
				RouteHandlerDelegate commandHandler;
				if (!PrepareAndValidateCommand(command, out commandHandler))
					return;

				// This could be null if Akka won't handle the command and something else will.
				if (commandHandler != null)
					commandHandler.Delegate(command);
			}
			// Let everything else know about the command (usually double handling a command is bad... but sometimes it might be useful... like pushing from AWS to Azure so both systems handle it... although an event really is the proper pattern to use here.
			CommandPublisher.Publish((IEnumerable<TCommand>)sourceCommands);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/>
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent PublishAndWait<TCommand, TEvent>(TCommand command, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return PublishAndWait<TCommand, TEvent>(command, -1, eventReceiver);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent PublishAndWait<TCommand, TEvent>(TCommand command, int millisecondsTimeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return PublishAndWait(command, events => (TEvent)events.SingleOrDefault(@event => @event is TEvent), millisecondsTimeout, eventReceiver);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent PublishAndWait<TCommand, TEvent>(TCommand command, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1L || num > int.MaxValue)
				throw new ArgumentOutOfRangeException("timeout", timeout, "SpinWait_SpinUntil_TimeoutWrong");
			return PublishAndWait<TCommand, TEvent>(command, (int)timeout.TotalMilliseconds, eventReceiver);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits until the specified condition is satisfied an event of <typeparamref name="TEvent"/>
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent PublishAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return PublishAndWait(command, condition, -1, eventReceiver);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent PublishAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, int millisecondsTimeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			if (eventReceiver != null)
				throw new NotSupportedException("Specifying a different event receiver is not yet supported.");

			TEvent result = (TEvent)(object)null;
			EventWaits.Add(command.CorrelationId, new List<IEvent<TAuthenticationToken>>());

			Publish(command);

			SpinWait.SpinUntil(() =>
			{
				IList<IEvent<TAuthenticationToken>> events = EventWaits[command.CorrelationId];

				result = condition(events);

				return result != null;
			}, millisecondsTimeout, SpinWait.DefaultSleepInMilliseconds);

			return result;
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent PublishAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1L || num > int.MaxValue)
				throw new ArgumentOutOfRangeException("timeout", timeout, "SpinWait_SpinUntil_TimeoutWrong");
			return PublishAndWait(command, condition, (int)timeout.TotalMilliseconds, eventReceiver);
		}

		#endregion

		#region Implementation of IHandlerRegistrar

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			Routes.RegisterHandler(handler, targetedType, holdMessageLock);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null);
		}

		#endregion
	}
}