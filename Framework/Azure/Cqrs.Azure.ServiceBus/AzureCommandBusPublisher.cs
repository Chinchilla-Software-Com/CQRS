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
using System.Globalization;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using Chinchilla.Logging;
using Cqrs.Bus;
using Cqrs.Events;
using Cqrs.Infrastructure;
using Cqrs.Messages;
#if NET452
using Microsoft.ServiceBus.Messaging;
#endif
#if NETSTANDARD2_0
using Microsoft.Azure.ServiceBus;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A <see cref="ICommandPublisher{TAuthenticationToken}"/> that resolves handlers , executes the handler and then publishes the <see cref="ICommand{TAuthenticationToken}"/> on the private command bus.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	// The “,nq” suffix here just asks the expression evaluator to remove the quotes when displaying the final value (nq = no quotes).
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AzureCommandBusPublisher<TAuthenticationToken>
		: AzureCommandBus<TAuthenticationToken>
		, IPublishAndWaitCommandPublisher<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="AzureCommandBusPublisher{TAuthenticationToken}"/>.
		/// </summary>
		public AzureCommandBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory, true)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.CommandBus.Publisher.UseApplicationInsightTelemetryHelper", correlationIdHelper);
		}

		/// <summary>
		/// The debugger variable window value.
		/// </summary>
		internal string DebuggerDisplay
		{
			get
			{
				string connectionString = string.Format("ConnectionString : {0}", MessageBusConnectionStringConfigurationKey);
				try
				{
					connectionString = string.Concat(connectionString, "=", GetConnectionString());
				}
				catch { /* */ }
				return string.Format(CultureInfo.InvariantCulture, "{0}, PrivateTopicName : {1}, PrivateTopicSubscriptionName : {2}, PublicTopicName : {3}, PublicTopicSubscriptionName : {4}",
					connectionString, PrivateTopicName, PrivateTopicSubscriptionName, PublicTopicName, PublicTopicSubscriptionName);
			}
		}

		#region Implementation of ICommandPublisher<TAuthenticationToken>

		/// <summary>
		/// Publishes the provided <paramref name="command"/> on the command bus.
		/// </summary>
		public virtual void Publish<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			if (command == null)
			{
				Logger.LogDebug("No command to publish.");
				return;
			}
			Type commandType = command.GetType();
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			bool mainWasSuccessfull = false;
			bool telemeterOverall = false;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			string telemetryName = string.Format("{0}/{1}/{2}", commandType.FullName, command.GetIdentity(), command.Id);
			var telemeteredCommand = command as ITelemeteredMessage;
			if (telemeteredCommand != null)
				telemetryName = telemeteredCommand.TelemetryName;
			else
				telemetryName = string.Format("Command/{0}", telemetryName);

			try
			{
				if (!AzureBusHelper.PrepareAndValidateCommand(command, "Azure-ServiceBus"))
					return;

				bool? isPublicBusRequired = BusHelper.IsPublicBusRequired(commandType);
				bool? isPrivateBusRequired = BusHelper.IsPrivateBusRequired(commandType);

				// We only add telemetry for overall operations if two occurred
				telemeterOverall = isPublicBusRequired != null && isPublicBusRequired.Value && isPrivateBusRequired != null && isPrivateBusRequired.Value;

				// Backwards compatibility and simplicity
				bool wasSuccessfull;
				Stopwatch stopWatch = Stopwatch.StartNew();
				if ((isPublicBusRequired == null || !isPublicBusRequired.Value) && (isPrivateBusRequired == null || !isPrivateBusRequired.Value))
				{
					stopWatch.Restart();
					responseCode = "200";
					wasSuccessfull = false;
					try
					{
						var brokeredMessage = CreateBrokeredMessage(MessageSerialiser.SerialiseCommand, commandType, command);
						int count = 1;
						do
						{
							try
							{
#if NET452
								PublicServiceBusPublisher.Send(brokeredMessage);
#endif
#if NETSTANDARD2_0
								PublicServiceBusPublisher.SendAsync(brokeredMessage).Wait();
#endif
								break;
							}
							catch (TimeoutException)
							{
								if (count >= TimeoutOnSendRetryMaximumCount)
									throw;
							}
							count++;
						} while (true);
						wasSuccessfull = true;
					}
					catch (QuotaExceededException exception)
					{
						responseCode = "429";
						Logger.LogError("The size of the command being sent was too large or the topic has reached it's limit.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
						throw;
					}
					catch (Exception exception)
					{
						responseCode = "500";
						Logger.LogError("An issue occurred while trying to publish a command.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
						throw;
					}
					finally
					{
						TelemetryHelper.TrackDependency("Azure/Servicebus/EventBus", "Command", telemetryName, "Default Bus", startedAt, stopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
					}
					Logger.LogDebug(string.Format("A command was published on the public bus with the id '{0}' was of type {1}.", command.Id, commandType.FullName));
				}
				if ((isPublicBusRequired != null && isPublicBusRequired.Value))
				{
					stopWatch.Restart();
					responseCode = "200";
					wasSuccessfull = false;
					try
					{
						var brokeredMessage = CreateBrokeredMessage(MessageSerialiser.SerialiseCommand, commandType, command);
						int count = 1;
						do
						{
							try
							{
#if NET452
								PublicServiceBusPublisher.Send(brokeredMessage);
#endif
#if NETSTANDARD2_0
								PublicServiceBusPublisher.SendAsync(brokeredMessage).Wait();
#endif
								break;
							}
							catch (TimeoutException)
							{
								if (count >= TimeoutOnSendRetryMaximumCount)
									throw;
							}
							count++;
						} while (true);
						wasSuccessfull = true;
					}
					catch (QuotaExceededException exception)
					{
						responseCode = "429";
						Logger.LogError("The size of the command being sent was too large or the topic has reached it's limit.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
						throw;
					}
					catch (Exception exception)
					{
						responseCode = "500";
						Logger.LogError("An issue occurred while trying to publish a command.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
						throw;
					}
					finally
					{
						TelemetryHelper.TrackDependency("Azure/Servicebus/EventBus", "Command", telemetryName, "Public Bus", startedAt, stopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
					}
					Logger.LogDebug(string.Format("A command was published on the public bus with the id '{0}' was of type {1}.", command.Id, commandType.FullName));
				}
				if (isPrivateBusRequired != null && isPrivateBusRequired.Value)
				{
					stopWatch.Restart();
					responseCode = "200";
					wasSuccessfull = false;
					try
					{
						var brokeredMessage = CreateBrokeredMessage(MessageSerialiser.SerialiseCommand, commandType, command);
						int count = 1;
						do
						{
							try
							{
#if NET452
								PrivateServiceBusPublisher.Send(brokeredMessage);
#endif
#if NETSTANDARD2_0
								PrivateServiceBusPublisher.SendAsync(brokeredMessage).Wait();
#endif
								break;
							}
							catch (TimeoutException)
							{
								if (count >= TimeoutOnSendRetryMaximumCount)
									throw;
							}
							count++;
						} while (true);
						wasSuccessfull = true;
					}
					catch (QuotaExceededException exception)
					{
						responseCode = "429";
						Logger.LogError("The size of the command being sent was too large or the topic has reached it's limit.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
						throw;
					}
					catch (Exception exception)
					{
						responseCode = "500";
						Logger.LogError("An issue occurred while trying to publish an command.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
						throw;
					}
					finally
					{
						TelemetryHelper.TrackDependency("Azure/Servicebus/EventBus", "Command", telemetryName, "Private Bus", startedAt, stopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
					}

					Logger.LogDebug(string.Format("An command was published on the private bus with the id '{0}' was of type {1}.", command.Id, commandType.FullName));
				}
				mainWasSuccessfull = true;
			}
			finally
			{
				mainStopWatch.Stop();
				if (telemeterOverall)
					TelemetryHelper.TrackDependency("Azure/Servicebus/CommandBus", "Command", telemetryName, null, startedAt, mainStopWatch.Elapsed, responseCode, mainWasSuccessfull, telemetryProperties);
			}
		}

		/// <summary>
		/// Publishes the provided <paramref name="commands"/> on the command bus.
		/// </summary>
		public virtual void Publish<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>
		{
			if (commands == null)
			{
				Logger.LogDebug("No commands to publish.");
				return;
			}
			IList<TCommand> sourceCommands = commands.ToList();
			if (!sourceCommands.Any())
			{
				Logger.LogDebug("An empty collection of commands to publish.");
				return;
			}

			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			bool mainWasSuccessfull = false;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			string telemetryName = "Commands";
			string telemetryNames = string.Empty;
			foreach (TCommand command in sourceCommands)
			{
				Type commandType = command.GetType();
				string subTelemetryName = string.Format("{0}/{1}", commandType.FullName, command.Id);
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
				IList<BrokeredMessage> privateBrokeredMessages = new List<BrokeredMessage>(sourceCommands.Count);
				IList<BrokeredMessage> publicBrokeredMessages = new List<BrokeredMessage>(sourceCommands.Count);
				foreach (TCommand command in sourceCommands)
				{
					if (!AzureBusHelper.PrepareAndValidateCommand(command, "Azure-ServiceBus"))
						continue;

					Type commandType = command.GetType();

					BrokeredMessage brokeredMessage = CreateBrokeredMessage(MessageSerialiser.SerialiseCommand, commandType, command);

					bool? isPublicBusRequired = BusHelper.IsPublicBusRequired(commandType);
					bool? isPrivateBusRequired = BusHelper.IsPrivateBusRequired(commandType);

					// Backwards compatibility and simplicity
					if ((isPublicBusRequired == null || !isPublicBusRequired.Value) && (isPrivateBusRequired == null || !isPrivateBusRequired.Value))
					{
						publicBrokeredMessages.Add(brokeredMessage);
						sourceCommandMessages.Add(string.Format("A command was published on the public bus with the id '{0}' was of type {1}.", command.Id, commandType.FullName));
					}
					if ((isPublicBusRequired != null && isPublicBusRequired.Value))
					{
						publicBrokeredMessages.Add(brokeredMessage);
						sourceCommandMessages.Add(string.Format("A command was published on the public bus with the id '{0}' was of type {1}.", command.Id, commandType.FullName));
					}
					if (isPrivateBusRequired != null && isPrivateBusRequired.Value)
					{
						privateBrokeredMessages.Add(brokeredMessage);
						sourceCommandMessages.Add(string.Format("A command was published on the private bus with the id '{0}' was of type {1}.", command.Id, commandType.FullName));
					}
				}

				bool wasSuccessfull;
				Stopwatch stopWatch = Stopwatch.StartNew();

				// Backwards compatibility and simplicity
				stopWatch.Restart();
				responseCode = "200";
				wasSuccessfull = false;
				try
				{
					int count = 1;
					do
					{
						try
						{
							if (publicBrokeredMessages.Any())
							{
#if NET452
								PublicServiceBusPublisher.SendBatch(publicBrokeredMessages);
#endif
#if NETSTANDARD2_0
								PublicServiceBusPublisher.SendAsync(publicBrokeredMessages).Wait();
#endif
							}
							else
								Logger.LogDebug("An empty collection of public commands to publish post validation.");
							break;
						}
						catch (TimeoutException)
						{
							if (count >= TimeoutOnSendRetryMaximumCount)
								throw;
						}
						count++;
					} while (true);
					wasSuccessfull = true;
				}
				catch (QuotaExceededException exception)
				{
					responseCode = "429";
					Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Command", publicBrokeredMessages } });
					throw;
				}
				catch (Exception exception)
				{
					responseCode = "500";
					Logger.LogError("An issue occurred while trying to publish a command.", exception: exception, metaData: new Dictionary<string, object> { { "Command", publicBrokeredMessages } });
					throw;
				}
				finally
				{
					TelemetryHelper.TrackDependency("Azure/Servicebus/CommandBus", "Command", telemetryName, "Public Bus", startedAt, stopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
				}

				stopWatch.Restart();
				responseCode = "200";
				wasSuccessfull = false;
				try
				{
					int count = 1;
					do
					{
						try
						{
							if (privateBrokeredMessages.Any())
							{
#if NET452
								PrivateServiceBusPublisher.SendBatch(privateBrokeredMessages);
#endif
#if NETSTANDARD2_0
								PrivateServiceBusPublisher.SendAsync(privateBrokeredMessages).Wait();
#endif
							}
							else
								Logger.LogDebug("An empty collection of private commands to publish post validation.");
							break;
						}
						catch (TimeoutException)
						{
							if (count >= TimeoutOnSendRetryMaximumCount)
								throw;
						}
						count++;
					} while (true);
					wasSuccessfull = true;
				}
				catch (QuotaExceededException exception)
				{
					responseCode = "429";
					Logger.LogError("The size of the event being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Command", privateBrokeredMessages } });
					throw;
				}
				catch (Exception exception)
				{
					responseCode = "500";
					Logger.LogError("An issue occurred while trying to publish a command.", exception: exception, metaData: new Dictionary<string, object> { { "Command", privateBrokeredMessages } });
					throw;
				}
				finally
				{
					TelemetryHelper.TrackDependency("Azure/Servicebus/CommandBus", "Command", telemetryName, "Private Bus", startedAt, stopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
				}

				foreach (string message in sourceCommandMessages)
					Logger.LogInfo(message);

				mainWasSuccessfull = true;
			}
			finally
			{
				mainStopWatch.Stop();
				TelemetryHelper.TrackDependency("Azure/Servicebus/CommandBus", "Command", telemetryName, null, startedAt, mainStopWatch.Elapsed, responseCode, mainWasSuccessfull, telemetryProperties);
			}
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
		public virtual TEvent PublishAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, int millisecondsTimeout,
			IEventReceiver<TAuthenticationToken> eventReceiver = null) where TCommand : ICommand<TAuthenticationToken>
		{
			if (command == null)
			{
				Logger.LogDebug("No command to publish.");
				return (TEvent)(object)null;
			}
			Type commandType = command.GetType();
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			bool wasSuccessfull = false;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			string telemetryName = string.Format("{0}/{1}", commandType.FullName, command.Id);
			var telemeteredCommand = command as ITelemeteredMessage;
			if (telemeteredCommand != null)
				telemetryName = telemeteredCommand.TelemetryName;
			telemetryName = string.Format("Command/{0}", telemetryName);

			TEvent result;

			try
			{
				if (eventReceiver != null)
					throw new NotSupportedException("Specifying a different event receiver is not yet supported.");
				if (!AzureBusHelper.PrepareAndValidateCommand(command, "Azure-ServiceBus"))
					return (TEvent)(object)null;

				result = (TEvent)(object)null;
				EventWaits.Add(command.CorrelationId, new List<IEvent<TAuthenticationToken>>());

				try
				{
					var brokeredMessage = CreateBrokeredMessage(MessageSerialiser.SerialiseCommand, commandType, command);
					int count = 1;
					do
					{
						try
						{
#if NET452
							PrivateServiceBusPublisher.Send(brokeredMessage);
#endif
#if NETSTANDARD2_0
							PrivateServiceBusPublisher.SendAsync(brokeredMessage).Wait();
#endif
							break;
						}
						catch (TimeoutException)
						{
							if (count >= TimeoutOnSendRetryMaximumCount)
								throw;
						}
						count++;
					} while (true);
				}
				catch (QuotaExceededException exception)
				{
					responseCode = "429";
					Logger.LogError("The size of the command being sent was too large.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
					throw;
				}
				catch (Exception exception)
				{
					responseCode = "500";
					Logger.LogError("An issue occurred while trying to publish a command.", exception: exception, metaData: new Dictionary<string, object> { { "Command", command } });
					throw;
				}
				Logger.LogInfo(string.Format("A command was sent of type {0}.", commandType.FullName));
				wasSuccessfull = true;
			}
			finally
			{
				mainStopWatch.Stop();
				TelemetryHelper.TrackDependency("Azure/Servicebus/CommandBus", "Command", telemetryName, null, startedAt, mainStopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
			}

			SpinWait.SpinUntil(() =>
			{
				IList<IEvent<TAuthenticationToken>> events = EventWaits[command.CorrelationId];

				result = condition(events);

				return result != null;
			}, millisecondsTimeout, sleepInMilliseconds: 1000);

			TelemetryHelper.TrackDependency("Azure/Servicebus/CommandBus", "Command/AndWait", string.Format("Command/AndWait{0}", telemetryName.Substring(7)), null, startedAt, mainStopWatch.Elapsed, responseCode, wasSuccessfull, telemetryProperties);
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
	}
}