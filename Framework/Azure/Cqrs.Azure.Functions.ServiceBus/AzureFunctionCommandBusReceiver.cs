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
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;

using Manager = Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient;
/*
using MessageLockLostException = Microsoft.Azure.ServiceBus.MessageLockLostException;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Azure.ServiceBus;
*/


namespace Cqrs.Azure.Functions.ServiceBus
{
	/// <summary>
	/// A <see cref="ICommandReceiver{TAuthenticationToken}"/> that receives network messages, resolves handlers and executes the handler.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	// The “,nq” suffix here just asks the expression evaluator to remove the quotes when displaying the final value (nq = no quotes).
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AzureFunctionCommandBusReceiver<TAuthenticationToken>
		: AzureFunctionCommandBus<TAuthenticationToken>
		, ICommandHandlerRegistrar
		, ICommandReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// The configuration key for
		/// the number of receiver <see cref="IMessageReceiver"/> instances to create
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string NumberOfReceiversCountConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.NumberOfReceiversCount"; }
		}

#if NETSTANDARD2_0
		/// <summary>
		/// Used by .NET Framework, but not .Net Core
		/// </summary>
#else
		/// <summary>
		/// The configuration key for
		/// <see cref="OnMessageOptions.MaxConcurrentCalls"/>.
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
#endif
		protected virtual string MaximumConcurrentReceiverProcessesCountConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.MaximumConcurrentReceiverProcessesCount"; }
		}

		/// <summary>
		/// The configuration key for
		/// the <see cref="SqlFilter.SqlExpression"/> that can be applied to
		/// the <see cref="IMessageReceiver"/> instances in the receivers
		/// as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected virtual string FilterKeyConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.Topics.Subscriptions.Filter"; }
		}

		/// <summary>
		/// The <see cref="SqlFilter.SqlExpression"/> that can be applied to
		/// the <see cref="IMessageReceiver"/> instances in the receivers,
		/// keyed by the topic name as there is the private and public bus
		/// </summary>
		protected IDictionary<string, string> FilterKey { get; set; }

		// ReSharper disable StaticMemberInGenericType
		/// <summary>
		/// Gets the <see cref="RouteManager"/>.
		/// </summary>
		public static RouteManager Routes { get; private set; }

		/// <summary>
		/// The number of handles currently being executed.
		/// </summary>
		protected static long CurrentHandles { get; set; }
		// ReSharper restore StaticMemberInGenericType

		static AzureFunctionCommandBusReceiver()
		{
			Routes = new RouteManager();
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureFunctionCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.CommandBus.Receiver.UseApplicationInsightTelemetryHelper", correlationIdHelper);
			FilterKey = new Dictionary<string, string>();
		}

		/// <summary>
		/// The debugger variable window value.
		/// </summary>
		internal string DebuggerDisplay
		{
			get
			{
				string connectionString = $"ConnectionString : {MessageBusConnectionStringConfigurationKey}";
				try
				{
					string _value = GetConnectionString();
					if (!string.IsNullOrWhiteSpace(_value))
						connectionString = string.Concat(connectionString, "=", _value);
					else
					{
						connectionString = $"ConnectionRBACSettings : ";
						connectionString = string.Concat(connectionString, "=", GetRbacConnectionSettings());
					}
				}
				catch { /* */ }
				return $"{connectionString}, PrivateTopicName : {PrivateTopicName}, PrivateTopicSubscriptionName : {PrivateTopicSubscriptionName}, PublicTopicName : {PublicTopicName}, PublicTopicSubscriptionName : {PublicTopicSubscriptionName}, FilterKey : {FilterKey}, NumberOfReceiversCount : {NumberOfReceiversCount}";
			}
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		/// <summary>
		/// Instantiate receiving on this bus by
		/// calling <see cref="CheckPrivateTopicExists"/> and <see cref="CheckPublicTopicExists"/>
		/// then InstantiateReceiving for private and public topics,
		/// calls <see cref="CleanUpDeadLetters"/> for the private and public topics,
		/// then calling <see cref="AzureBus{TAuthenticationToken}.StartSettingsChecking"/>
		/// </summary>
		protected override void InstantiateReceiving()
		{
			// https://docs.microsoft.com/en-us/azure/application-insights/app-insights-analytics-reference#summarize-operator
			// http://www.summa.com/blog/business-blog/everything-you-need-to-know-about-azure-service-bus-brokered-messaging-part-2#rulesfiltersactions
			// https://github.com/Azure-Samples/azure-servicebus-messaging-samples/tree/master/TopicFilters
			ServiceBusClient client;
			string connectionString = ConnectionString;
			AzureBusRbacSettings rbacSettings = RbacConnectionSettings;

			if (!string.IsNullOrWhiteSpace(connectionString))
				client = new ServiceBusClient(ConnectionString);
			else
			{
				var credentials = new ClientSecretCredential(rbacSettings.TenantId, rbacSettings.ApplicationId, rbacSettings.ClientKey);
				client = new ServiceBusClient(rbacSettings.Endpoint, credentials);
			}

			Task.Run(async () => {
				Manager manager;
				if (!string.IsNullOrWhiteSpace(connectionString))
					manager = new Manager(ConnectionString);
				else
				{
					var credentials = new ClientSecretCredential(rbacSettings.TenantId, rbacSettings.ApplicationId, rbacSettings.ClientKey);
					manager = new Manager(rbacSettings.Endpoint, credentials);
				}

				await CheckPrivateTopicExistsAsync(manager);
				await CheckPublicTopicExistsAsync(manager);
			}).Wait();

			try
			{
				Task.Run(async () => {
					await InstantiateReceivingAsync(client, PrivateServiceBusReceivers, PrivateTopicName, PrivateTopicSubscriptionName);
				}).Wait();
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the private Service Bus receivers may be invalid.", exception);
			}
			try
			{
				Task.Run(async () => {
					await InstantiateReceivingAsync(client, PublicServiceBusReceivers, PublicTopicName, PublicTopicSubscriptionName);
				}).Wait();
			}
			catch (UriFormatException exception)
			{
				throw new InvalidConfigurationException("The connection string for one of the public Service Bus receivers may be invalid.", exception);
			}

			bool enableDeadLetterCleanUp;
			string enableDeadLetterCleanUpValue = ConfigurationManager.GetSetting("Cqrs.Azure.Servicebus.EnableDeadLetterCleanUp");
			if (bool.TryParse(enableDeadLetterCleanUpValue, out enableDeadLetterCleanUp) && enableDeadLetterCleanUp)
			{
				Task.Run(async () => {
					await CleanUpDeadLettersAsync(client, PrivateTopicName, PrivateTopicSubscriptionName);
					await CleanUpDeadLettersAsync(client, PublicTopicName, PublicTopicSubscriptionName);
				}).Wait();
			}

			// If this is also a publisher, then it will the check over there and that will handle this
			// we only need to check one of these
			if (PublicServiceBusPublisher != null)
				return;

			StartSettingsChecking();
		}

		#endregion

		/// <summary>
		/// Register a command handler that will listen and respond to commands.
		/// </summary>
		/// <remarks>
		/// In many cases the <paramref name="targetedType"/> will be the handler class itself, what you actually want is the target of what is being updated.
		/// </remarks>
		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			AzureBusHelper.RegisterHandler(TelemetryHelper, Routes, handler, targetedType, holdMessageLock);
		}

		/// <summary>
		/// Register a command handler that will listen and respond to commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		/// <summary>
		/// Receives a <see cref="ServiceBusReceivedMessage"/> from the command bus.
		/// </summary>
		public virtual void ReceiveCommand(ServiceBusReceiver messageReceiver, ServiceBusReceivedMessage message)
		{
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			// Null means it was skipped
			bool? wasSuccessfull = true;
			string telemetryName = string.Format("Cqrs/Handle/Command/{0}", message.MessageId);
			ISingleSignOnToken authenticationToken = null;
			Guid? guidAuthenticationToken = null;
			string stringAuthenticationToken = null;
			int? intAuthenticationToken = null;

			IDictionary<string, string> telemetryProperties = ExtractTelemetryProperties(message, "Azure/Servicebus");
			TelemetryHelper.TrackMetric("Cqrs/Handle/Command", CurrentHandles++, telemetryProperties);
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			try
			{
				try
				{
					Logger.LogDebug(string.Format("A command message arrived with the id '{0}'.", message.MessageId));
					string messageBody = message.GetBodyAsString();

					ICommand<TAuthenticationToken> command = AzureBusHelper.ReceiveCommand(
						/*messageReceiver*/ null
						, messageBody, ReceiveCommand,
						string.Format("id '{0}'", message.MessageId),
						ExtractSignature(message),
						SigningTokenConfigurationKey,
						() =>
						{
							wasSuccessfull = null;
							telemetryName = string.Format("Cqrs/Handle/Command/Skipped/{0}", message.MessageId);
							responseCode = "204";

							try
							{
								messageReceiver.CompleteMessageAsync(message).Wait(1500);
							}
							catch (AggregateException aggregateException)
							{
								if (aggregateException.InnerException is ServiceBusException)
									throw aggregateException.InnerException;
								else
									throw;
							}

							Logger.LogDebug(string.Format("A command message arrived with the id '{0}' but processing was skipped due to event settings.", message.MessageId));
							TelemetryHelper.TrackEvent("Cqrs/Handle/Command/Skipped", telemetryProperties);
						},
						() =>
						{
							// Apparently locks are renewed automatically
							// see https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp#peeklock-behavior
							// ignore all of that and let's do this manually anyways

							AzureBusHelper.RefreshLockAsync(Logger, messageReceiver, brokeredMessageRenewCancellationTokenSource, message, "command");
						}
					);

					if (wasSuccessfull != null)
					{
						if (command != null)
						{
							telemetryName = string.Format("{0}/{1}", command.GetType().FullName, command.Id);
							authenticationToken = command.AuthenticationToken as ISingleSignOnToken;
							if (AuthenticationTokenIsGuid)
								guidAuthenticationToken = command.AuthenticationToken as Guid?;
							if (AuthenticationTokenIsString)
								stringAuthenticationToken = command.AuthenticationToken as string;
							if (AuthenticationTokenIsInt)
								intAuthenticationToken = command.AuthenticationToken as int?;

							var telemeteredMessage = command as ITelemeteredMessage;
							if (telemeteredMessage != null)
								telemetryName = telemeteredMessage.TelemetryName;

							telemetryName = string.Format("Cqrs/Handle/Command/{0}", telemetryName);
						}
						// Remove message from queue
						try
						{
							messageReceiver.CompleteMessageAsync(message).Wait(1500);
						}
						catch (AggregateException aggregateException)
						{
							if (aggregateException.InnerException is ServiceBusException)
								throw aggregateException.InnerException;
							else
								throw;
						}
					}
					Logger.LogDebug(string.Format("A command message arrived and was processed with the id '{0}'.", message.MessageId));
				}
				catch (AggregateException aggregateException)
				{
					throw aggregateException.InnerException;
				}
			}
			catch (ServiceBusException exception)
			{
				IDictionary<string, string> subTelemetryProperties = new Dictionary<string, string>(telemetryProperties);
				subTelemetryProperties.Add("TimeTaken", mainStopWatch.Elapsed.ToString());
				TelemetryHelper.TrackException(exception, null, subTelemetryProperties);
				if (ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete)
				{
					Logger.LogError(exception.Message, exception: exception);
					// Indicates a problem, unlock message in queue
					messageReceiver.AbandonMessageAsync(message).Wait(1500);
					wasSuccessfull = false;
				}
				else
				{
					Logger.LogWarning(exception.Message, exception: exception);
					try
					{
						messageReceiver.DeadLetterMessageAsync(message, "LockLostButHandled", "The message was handled but the lock was lost.").Wait(1500);
					}
					catch (Exception)
					{
						// Oh well, move on.
						messageReceiver.AbandonMessageAsync(message).Wait(1500);
					}
				}
				responseCode = "599";
			}
			catch (UnAuthorisedMessageReceivedException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but was not authorised.", message.MessageId), exception: exception);
				messageReceiver.DeadLetterMessageAsync(message, "UnAuthorisedMessageReceivedException", exception.Message).Wait(1500);
				wasSuccessfull = false;
				responseCode = "401";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
				throw;
			}
			catch (NoHandlersRegisteredException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but no handlers were found to process it.", message.MessageId), exception: exception);
				messageReceiver.DeadLetterMessageAsync(message, "NoHandlersRegisteredException", exception.Message).Wait(1500);
				wasSuccessfull = false;
				responseCode = "501";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
			}
			catch (NoHandlerRegisteredException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but no handler was found to process it.", message.MessageId), exception: exception);
				messageReceiver.DeadLetterMessageAsync(message, "NoHandlerRegisteredException", exception.Message).Wait(1500);
				wasSuccessfull = false;
				responseCode = "501";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
			}
			catch (Exception exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
				messageReceiver.AbandonMessageAsync(message).Wait(1500);
				wasSuccessfull = false;
				responseCode = "500";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
			}
			finally
			{
				// Cancel the lock of renewing the task
				brokeredMessageRenewCancellationTokenSource.Cancel();
				TelemetryHelper.TrackMetric("Cqrs/Handle/Command", CurrentHandles--, telemetryProperties);

				mainStopWatch.Stop();
				if (guidAuthenticationToken != null)
					TelemetryHelper.TrackRequest
					(
						telemetryName,
						guidAuthenticationToken,
						startedAt,
						mainStopWatch.Elapsed,
						responseCode,
						wasSuccessfull == null || wasSuccessfull.Value,
						telemetryProperties
					);
				else if (intAuthenticationToken != null)
					TelemetryHelper.TrackRequest
					(
						telemetryName,
						intAuthenticationToken,
						startedAt,
						mainStopWatch.Elapsed,
						responseCode,
						wasSuccessfull == null || wasSuccessfull.Value,
						telemetryProperties
					);
				else if (stringAuthenticationToken != null)
					TelemetryHelper.TrackRequest
					(
						telemetryName,
						stringAuthenticationToken,
						startedAt,
						mainStopWatch.Elapsed,
						responseCode,
						wasSuccessfull == null || wasSuccessfull.Value,
						telemetryProperties
					);
				else
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

		/// <summary>
		/// Receives a <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		public virtual bool? ReceiveCommand(ICommand<TAuthenticationToken> command)
		{
			return AzureBusHelper.DefaultReceiveCommand(command, Routes, "Azure-Function-ServiceBus");
		}

		#region Overrides of AzureBus<TAuthenticationToken>

		/// <summary>
		/// Returns <see cref="NumberOfReceiversCountConfigurationKey"/> from <see cref="IConfigurationManager"/> 
		/// if no value is set, returns <see cref="AzureBus{TAuthenticationToken}.DefaultNumberOfReceiversCount"/>.
		/// </summary>
		protected override int GetCurrentNumberOfReceiversCount()
		{
			string numberOfReceiversCountValue;
			int numberOfReceiversCount;
			if (ConfigurationManager.TryGetSetting(NumberOfReceiversCountConfigurationKey, out numberOfReceiversCountValue) && !string.IsNullOrWhiteSpace(numberOfReceiversCountValue))
			{
				if (!int.TryParse(numberOfReceiversCountValue, out numberOfReceiversCount))
					numberOfReceiversCount = DefaultNumberOfReceiversCount;
			}
			else
				numberOfReceiversCount = DefaultNumberOfReceiversCount;
			return numberOfReceiversCount;
		}

		/// <summary>
		/// Returns <see cref="MaximumConcurrentReceiverProcessesCountConfigurationKey"/> from <see cref="IConfigurationManager"/> 
		/// if no value is set, returns <see cref="AzureBus{TAuthenticationToken}.DefaultMaximumConcurrentReceiverProcessesCount"/>.
		/// </summary>
		protected override int GetCurrentMaximumConcurrentReceiverProcessesCount()
		{
			string maximumConcurrentReceiverProcessesCountValue;
			int maximumConcurrentReceiverProcessesCount;
			if (ConfigurationManager.TryGetSetting(MaximumConcurrentReceiverProcessesCountConfigurationKey, out maximumConcurrentReceiverProcessesCountValue) && !string.IsNullOrWhiteSpace(maximumConcurrentReceiverProcessesCountValue))
			{
				if (!int.TryParse(maximumConcurrentReceiverProcessesCountValue, out maximumConcurrentReceiverProcessesCount))
					maximumConcurrentReceiverProcessesCount = DefaultMaximumConcurrentReceiverProcessesCount;
			}
			else
				maximumConcurrentReceiverProcessesCount = DefaultMaximumConcurrentReceiverProcessesCount;
			return maximumConcurrentReceiverProcessesCount;
		}

		#endregion

		#region Implementation of ICommandReceiver

		/// <summary>
		/// Starts listening and processing instances of <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		public void Start()
		{
			InstantiateReceiving();

			// Configure the callback options
			var options = new ServiceBusProcessorOptions
			{
				// By default or when AutoCompleteMessages is set to true, the processor will complete the message after executing the message handler
				// Set AutoCompleteMessages to false to [settle messages](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-transfers-locks-settlement#peeklock) on your own.
				// In both cases, if the message handler throws an exception without settling the message, the processor will abandon the message.
				AutoCompleteMessages = false,

				ReceiveMode = ServiceBusReceiveMode.PeekLock,

				MaxConcurrentCalls = NumberOfReceiversCount,

				Identifier = Logger.LoggerSettings.ModuleName
			};

			// Callback to handle received messages
			RegisterReceiverMessageHandlerAsync(ReceiveCommand, options);
		}

		#endregion


		protected override void InstantiatePublishing()
		{
			base.InstantiatePublishing();
		}

		protected override void TriggerSettingsChecking()
		{
			base.TriggerSettingsChecking();
		}

		protected override void ApplyReceiverMessageHandler()
		{
			base.ApplyReceiverMessageHandler();
		}
	}
}