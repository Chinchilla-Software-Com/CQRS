using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Bus;
using Cqrs.Events;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusPublisher<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ISendAndWaitCommandSender<TAuthenticationToken>
	{
		protected IDependencyResolver DependencyResolver { get; private set; }

		public AzureCommandBusPublisher(IConfigurationManager configurationManager, IBusHelper busHelper, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IDependencyResolver dependencyResolver)
			: base(configurationManager, busHelper, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, true)
		{
			DependencyResolver = dependencyResolver;
		}

		protected virtual void PrepareCommand<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			if (command.AuthenticationToken == null)
				command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			if (string.IsNullOrWhiteSpace(command.OriginatingFramework))
				command.OriginatingFramework = "Azure";
			IList<string> frameworks = new List<string>(command.Frameworks);
			frameworks.Add("Azure");
			command.Frameworks = frameworks;
		}

		protected virtual bool PrepareAndValidateCommand<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Type commandType = command.GetType();

			if (command.Frameworks.Contains("Azure"))
			{
				Logger.LogInfo("The provided command has already been processed in Azure.", string.Format("{0}\\Handle({1})", GetType().FullName, commandType.FullName));
				return false;
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
				return false;
			}

			PrepareCommand(command);
			return true;
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			if (!PrepareAndValidateCommand(command))
				return;

			ServiceBusPublisher.Send(new BrokeredMessage(MessageSerialiser.SerialiseCommand(command)));
			Logger.LogInfo(string.Format("A command was sent of type {0}.", command.GetType().FullName));
		}

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/>
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent SendAndWait<TCommand, TEvent>(TCommand command, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return SendAndWait<TCommand, TEvent>(command, -1, eventReceiver);
		}

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent SendAndWait<TCommand, TEvent>(TCommand command, int millisecondsTimeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return SendAndWait(command, events => (TEvent)events.SingleOrDefault(@event => @events is TEvent), millisecondsTimeout, eventReceiver);
		}

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent SendAndWait<TCommand, TEvent>(TCommand command, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1L || num > int.MaxValue)
				throw new ArgumentOutOfRangeException("timeout", timeout, "SpinWait_SpinUntil_TimeoutWrong");
			return SendAndWait<TCommand, TEvent>(command, (int)timeout.TotalMilliseconds, eventReceiver);
		}

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits until the specified condition is satisfied an event of <typeparamref name="TEvent"/>
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return SendAndWait(command, condition, -1, eventReceiver);
		}

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, int millisecondsTimeout,
			IEventReceiver<TAuthenticationToken> eventReceiver = null) where TCommand : ICommand<TAuthenticationToken>
		{
			if (eventReceiver != null)
				throw new NotSupportedException("Specifying a different event receiver is not yet supported.");
			if (!PrepareAndValidateCommand(command))
				return (TEvent)(object)null;

			TEvent result = (TEvent)(object)null;
			EventWaits.Add(command.CorrelationId, new List<IEvent<TAuthenticationToken>>());

			ServiceBusPublisher.Send(new BrokeredMessage(MessageSerialiser.SerialiseCommand(command)));
			Logger.LogInfo(string.Format("A command was sent of type {0}.", command.GetType().FullName));

			SpinWait.SpinUntil(() =>
			{
				IList<IEvent<TAuthenticationToken>> events = EventWaits[command.CorrelationId];

				result = condition(events);

				return result != null;
			}, millisecondsTimeout);

			return result;
		}

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1L || num > int.MaxValue)
				throw new ArgumentOutOfRangeException("timeout", timeout, "SpinWait_SpinUntil_TimeoutWrong");
			return SendAndWait(command, condition, (int)timeout.TotalMilliseconds, eventReceiver);
		}

		#endregion
	}
}
