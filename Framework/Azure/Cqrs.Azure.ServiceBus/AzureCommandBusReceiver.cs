#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusReceiver<TAuthenticationToken>
		: AzureCommandBus<TAuthenticationToken>
		, ICommandHandlerRegistrar
		, ICommandReceiver<TAuthenticationToken>
	{
		protected virtual string FilterKeyConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.TopicName.SubscriptionName.Filter"; }
		}

		// ReSharper disable StaticMemberInGenericType
		protected static RouteManager Routes { get; private set; }

		protected static long CurrentHandles { get; set; }
		// ReSharper restore StaticMemberInGenericType

		static AzureCommandBusReceiver()
		{
			Routes = new RouteManager();
		}

		public AzureCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.CommandBus.Receiver.UseApplicationInsightTelemetryHelper", correlationIdHelper);
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		protected override void InstantiateReceiving(IDictionary<int, SubscriptionClient> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			base.InstantiateReceiving(serviceBusReceivers, topicName, topicSubscriptionName);

			// https://docs.microsoft.com/en-us/azure/application-insights/app-insights-analytics-reference#summarize-operator
			// http://www.summa.com/blog/business-blog/everything-you-need-to-know-about-azure-service-bus-brokered-messaging-part-2#rulesfiltersactions
			// https://github.com/Azure-Samples/azure-servicebus-messaging-samples/tree/master/TopicFilters
			SubscriptionClient client = serviceBusReceivers[0];
			try
			{
				client.RemoveRule("CqrsConfiguredFilter");
			}
			catch (MessagingEntityNotFoundException)
			{
			}

			try
			{
				string filter = ConfigurationManager.GetSetting(FilterKeyConfigurationKey);
				if (!string.IsNullOrWhiteSpace(filter))
				{
					RuleDescription ruleDescription = new RuleDescription
					(
						"CqrsConfiguredFilter",
						new SqlFilter(filter)
					);
					client.AddRuleAsync(ruleDescription);
				}
			}
			catch (Exception exception)
			{
				Logger.LogError("Setting the filter failed.", exception: exception);
				TelemetryHelper.TrackException(exception);
			}
		}

		#endregion

		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			AzureBusHelper.RegisterHandler(TelemetryHelper, Routes, handler, targetedType, holdMessageLock);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		protected virtual void ReceiveCommand(BrokeredMessage message)
		{
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			// Null means it was skipped
			bool? wasSuccessfull = true;
			string telemetryName = string.Format("Cqrs/Handle/Command/{0}", message.MessageId);
			ISingleSignOnToken authenticationToken = null;

			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			object value;
			if (message.Properties.TryGetValue("Type", out value))
				telemetryProperties.Add("MessageType", value.ToString());
			TelemetryHelper.TrackMetric("Cqrs/Handle/Command", CurrentHandles++, telemetryProperties);
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			try
			{
				Logger.LogDebug(string.Format("A command message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBody<string>();


				ICommand<TAuthenticationToken> command = AzureBusHelper.ReceiveCommand(messageBody, ReceiveCommand,
					string.Format("id '{0}'", message.MessageId),
					() =>
					{
						wasSuccessfull = null;
						telemetryName = string.Format("Cqrs/Handle/Command/Skipped/{0}", message.MessageId);
						responseCode = "204";
						// Remove message from queue
						try
						{
							message.Complete();
						}
						catch (MessageLockLostException exception)
						{
							throw new MessageLockLostException(string.Format("The lock supplied for the skipped command message '{0}' is invalid.", message.MessageId), exception);
						}
						Logger.LogDebug(string.Format("A command message arrived with the id '{0}' but processing was skipped due to event settings.", message.MessageId));
						TelemetryHelper.TrackEvent("Cqrs/Handle/Command/Skipped", telemetryProperties);
					},
					() =>
					{
						AzureBusHelper.RefreshLock(brokeredMessageRenewCancellationTokenSource, message, "command");
					}
				);

				if (wasSuccessfull != null)
				{
					if (command != null)
					{
						telemetryName = string.Format("{0}/{1}", command.GetType().FullName, command.Id);
						authenticationToken = command.AuthenticationToken as ISingleSignOnToken;

						var telemeteredMessage = command as ITelemeteredMessage;
						if (telemeteredMessage != null)
							telemetryName = telemeteredMessage.TelemetryName;

						telemetryName = string.Format("Cqrs/Handle/Command/{0}", telemetryName);
					}
					// Remove message from queue
					try
					{
						message.Complete();
					}
					catch (MessageLockLostException exception)
					{
						throw new MessageLockLostException(string.Format("The lock supplied for command '{0}' of type {1} is invalid.", command.Id, command.GetType().Name), exception);
					}
				}
				Logger.LogDebug(string.Format("A command message arrived and was processed with the id '{0}'.", message.MessageId));
			}
			catch (MessageLockLostException exception)
			{
				IDictionary<string, string> subTelemetryProperties = new Dictionary<string, string>(telemetryProperties);
				subTelemetryProperties.Add("TimeTaken", mainStopWatch.Elapsed.ToString());
				TelemetryHelper.TrackException(exception, null, subTelemetryProperties);
				if (ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete)
				{
					Logger.LogError(exception.Message, exception: exception);
					// Indicates a problem, unlock message in queue
					message.Abandon();
					wasSuccessfull = false;
				}
				else
				{
					Logger.LogWarning(exception.Message, exception: exception);
					try
					{
						message.DeadLetter("LockLostButHandled", "The message was handled but the lock was lost.");
					}
					catch (Exception)
					{
						// Oh well, move on.
						message.Abandon();
					}
				}
				responseCode = "599";
			}
			catch (Exception exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
				message.Abandon();
				wasSuccessfull = false;
				responseCode = "500";
			}
			finally
			{
				// Cancel the lock of renewing the task
				brokeredMessageRenewCancellationTokenSource.Cancel();
				TelemetryHelper.TrackMetric("Cqrs/Handle/Command", CurrentHandles--, telemetryProperties);

				mainStopWatch.Stop();
				TelemetryHelper.TrackRequest
				(
					telemetryName,
					authenticationToken,
					startedAt,
					mainStopWatch.Elapsed,
					responseCode,
					wasSuccessfull == null || wasSuccessfull.Value,
					telemetryProperties
				);

				TelemetryHelper.Flush();
			}
		}

		public virtual bool? ReceiveCommand(ICommand<TAuthenticationToken> command)
		{
			return AzureBusHelper.DefaultReceiveCommand(command, Routes, "Azure-ServiceBus");
		}

		#region Implementation of ICommandReceiver

		public void Start()
		{
			InstantiateReceiving();

			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
			};

			// Callback to handle received messages
			RegisterReceiverMessageHandler(ReceiveCommand, options);
		}

		#endregion
	}
}