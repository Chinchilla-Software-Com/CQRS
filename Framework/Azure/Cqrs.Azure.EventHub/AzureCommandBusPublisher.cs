#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Infrastructure;
using Cqrs.Messages;
using EventData = Microsoft.ServiceBus.Messaging.EventData;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusPublisher<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ISendAndWaitCommandSender<TAuthenticationToken>
	{
		public AzureCommandBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, true)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.EventHub.EventBus.Publisher.UseApplicationInsightTelemetryHelper", correlationIdHelper);
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public virtual void Publish<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			bool wasSuccessfull = false;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/EventHub" } };
			string telemetryName = string.Format("{0}/{1}", command.GetType().FullName, command.Id);
			var telemeteredCommand = command as ITelemeteredMessage;
			if (telemeteredCommand != null)
				telemetryName = telemeteredCommand.TelemetryName;
			telemetryName = string.Format("Command/{0}", telemetryName);

			try
			{
				if (!AzureBusHelper.PrepareAndValidateCommand(command, "Azure-EventHub"))
					return;

				var brokeredMessage = new EventData(Encoding.UTF8.GetBytes(MessageSerialiser.SerialiseCommand(command)));
				brokeredMessage.Properties.Add("Type", command.GetType().FullName);

				try
				{
					EventHubPublisher.Send(brokeredMessage);
				}
				catch (Exception exception)
				{
					responseCode = "500";
					Logger.LogError("An issue occurred while trying to publish a command.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
					throw;
				}
				Logger.LogInfo(string.Format("A command was sent of type {0}.", command.GetType().FullName));
				wasSuccessfull = true;
			}
			finally
			{
				mainStopWatch.Stop();
				TelemetryHelper.TrackDependency("Azure/EventHub/CommandBus", "Command", telemetryName, null, startedAt, mainStopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
			}
		}

		public virtual void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Publish(command);
		}

		public virtual void Publish<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>
		{
			IList<TCommand> sourceCommands = commands.ToList();

			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			bool wasSuccessfull = false;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/EventHub" } };
			string telemetryName = "Commands";
			string telemetryNames = string.Empty;
			foreach (TCommand command in sourceCommands)
			{
				string subTelemetryName = string.Format("{0}/{1}", command.GetType().FullName, command.Id);
				var telemeteredCommand = command as ITelemeteredMessage;
				if (telemeteredCommand != null)
					subTelemetryName = telemeteredCommand.TelemetryName;
				telemetryNames = string.Format("{0}{1},", telemetryNames, subTelemetryName);
			}
			if (telemetryNames.Length > 0)
				telemetryNames = telemetryNames.Substring(0, telemetryNames.Length - 1);
			telemetryProperties.Add("Commands", telemetryNames);

			try
			{
				IList<string> sourceCommandMessages = new List<string>();
				IList<EventData> brokeredMessages = new List<EventData>(sourceCommands.Count);
				foreach (TCommand command in sourceCommands)
				{
					if (!AzureBusHelper.PrepareAndValidateCommand(command, "Azure-EventHub"))
						continue;

					var brokeredMessage = new EventData(Encoding.UTF8.GetBytes(MessageSerialiser.SerialiseCommand(command)));
					brokeredMessage.Properties.Add("Type", command.GetType().FullName);

					brokeredMessages.Add(brokeredMessage);
					sourceCommandMessages.Add(string.Format("A command was sent of type {0}.", command.GetType().FullName));
				}

				try
				{
					EventHubPublisher.SendBatch(brokeredMessages);
				}
				catch (Exception exception)
				{
					responseCode = "500";
					Logger.LogError("An issue occurred while trying to publish a command.", exception: exception, metaData: new Dictionary<string, object> { { "Commands", sourceCommands } });
					throw;
				}

				foreach (string message in sourceCommandMessages)
					Logger.LogInfo(message);

				wasSuccessfull = true;
			}
			finally
			{
				mainStopWatch.Stop();
				TelemetryHelper.TrackDependency("Azure/EventHub/CommandBus", "Command", telemetryName, null, startedAt, mainStopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
			}
		}

		public virtual void Send<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Publish(commands);
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
		public virtual TEvent SendAndWait<TCommand, TEvent>(TCommand command, int millisecondsTimeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return SendAndWait(command, events => (TEvent)events.SingleOrDefault(@event => @event is TEvent), millisecondsTimeout, eventReceiver);
		}

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent SendAndWait<TCommand, TEvent>(TCommand command, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
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
		public virtual TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, IEventReceiver<TAuthenticationToken> eventReceiver = null)
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
		public virtual TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, int millisecondsTimeout,
			IEventReceiver<TAuthenticationToken> eventReceiver = null) where TCommand : ICommand<TAuthenticationToken>
		{
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			bool wasSuccessfull = false;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/EventHub" } };
			string telemetryName = string.Format("{0}/{1}", command.GetType().FullName, command.Id);
			var telemeteredCommand = command as ITelemeteredMessage;
			if (telemeteredCommand != null)
				telemetryName = telemeteredCommand.TelemetryName;
			telemetryName = string.Format("Command/{0}", telemetryName);

			TEvent result;

			try
			{
				if (eventReceiver != null)
					throw new NotSupportedException("Specifying a different event receiver is not yet supported.");
				if (!AzureBusHelper.PrepareAndValidateCommand(command, "Azure-EventHub"))
					return (TEvent)(object)null;

				result = (TEvent)(object)null;
				EventWaits.Add(command.CorrelationId, new List<IEvent<TAuthenticationToken>>());

				try
				{
					EventHubPublisher.Send(new EventData(Encoding.UTF8.GetBytes(MessageSerialiser.SerialiseCommand(command))));
				}
				catch (Exception exception)
				{
					responseCode = "500";
					Logger.LogError("An issue occurred while trying to publish a command.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
					throw;
				}
				Logger.LogInfo(string.Format("A command was sent of type {0}.", command.GetType().FullName));
				wasSuccessfull = true;
			}
			finally
			{
				mainStopWatch.Stop();
				TelemetryHelper.TrackDependency("Azure/EventHub/CommandBus", "Command", telemetryName, null, startedAt, mainStopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
			}

			SpinWait.SpinUntil(() =>
			{
				IList<IEvent<TAuthenticationToken>> events = EventWaits[command.CorrelationId];

				result = condition(events);

				return result != null;
			}, millisecondsTimeout, 1000);

			TelemetryHelper.TrackDependency("Azure/EventHub/CommandBus", "Command/AndWait", string.Format("Command/AndWait{0}", telemetryName.Substring(7)), null, startedAt, mainStopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties); 
			return result;
		}

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
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
