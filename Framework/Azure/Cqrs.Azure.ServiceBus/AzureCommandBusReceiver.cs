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
using System.Threading;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Exceptions;
using Cqrs.Messages;

#if NETSTANDARD2_0 || NET48_OR_GREATER
using Azure.Messaging.ServiceBus;
using System.Reflection;
using BrokeredMessage = Azure.Messaging.ServiceBus.ServiceBusReceivedMessage;
using IMessageReceiver = Azure.Messaging.ServiceBus.ServiceBusProcessor;
using Manager = Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient;
using RuleDescription = Azure.Messaging.ServiceBus.Administration.RuleProperties;
using SqlFilter = Azure.Messaging.ServiceBus.Administration.SqlRuleFilter;
#else
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Manager = Microsoft.ServiceBus.NamespaceManager;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A <see cref="ICommandReceiver{TAuthenticationToken}"/> that receives network messages, resolves handlers and executes the handler.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	// The “,nq” suffix here just asks the expression evaluator to remove the quotes when displaying the final value (nq = no quotes).
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AzureCommandBusReceiver<TAuthenticationToken>
		: AzureCommandBus<TAuthenticationToken>
#if NETSTANDARD2_0 || NET48_OR_GREATER
		, IAsyncCommandHandlerRegistrar
		, IAsyncCommandReceiver<TAuthenticationToken>
#else
		, ICommandHandlerRegistrar
		, ICommandReceiver<TAuthenticationToken>
#endif
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

#if NETSTANDARD2_0 || NET48_OR_GREATER
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

		static AzureCommandBusReceiver()
		{
			Routes = new RouteManager();
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
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
					string _value =
#if NETSTANDARD2_0 || NET48_OR_GREATER
						GetConnectionStringAsync().Result;
#else
						GetConnectionString();
#endif
					if (!string.IsNullOrWhiteSpace(_value))
						connectionString = string.Concat(connectionString, "=", _value);
					else
					{
						connectionString = $"ConnectionRBACSettings : ";
						connectionString = string.Concat(connectionString, "=",
#if NETSTANDARD2_0 || NET48_OR_GREATER
							GetRbacConnectionSettingsAsync().Result
#else
							GetRbacConnectionSettings()
#endif
						);
					}
				}
				catch { /* */ }
				return $"{connectionString}, PrivateTopicName : {PrivateTopicName}, PrivateTopicSubscriptionName : {PrivateTopicSubscriptionName}, PublicTopicName : {PublicTopicName}, PublicTopicSubscriptionName : {PublicTopicSubscriptionName}, FilterKey : {FilterKey}, NumberOfReceiversCount : {NumberOfReceiversCount}";
			}
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

#if NETSTANDARD2_0 || NET48_OR_GREATER
		private static SemaphoreSlim lockObject = new SemaphoreSlim(1, 1);
#else
		private object lockObject = new object();
#endif
#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Calls <see cref="AzureServiceBus{TAuthenticationToken}.InstantiateReceivingAsync()"/>
		/// then uses a <see cref="Task"/> to apply the <see cref="FilterKey"/> as a <see cref="RuleDescription"/>
		/// to the <see cref="IMessageReceiver"/> instances in <paramref name="serviceBusReceivers"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="IMessageReceiver"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
#else
		/// <summary>
		/// Calls <see cref="AzureServiceBus{TAuthenticationToken}.InstantiateReceiving()"/>
		/// then uses a <see cref="Task"/> to apply the <see cref="FilterKey"/> as a <see cref="RuleDescription"/>
		/// to the <see cref="IMessageReceiver"/> instances in <paramref name="serviceBusReceivers"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		/// <param name="serviceBusReceivers">The receivers collection to place <see cref="IMessageReceiver"/> instances into.</param>
		/// <param name="topicName">The topic name.</param>
		/// <param name="topicSubscriptionName">The topic subscription name.</param>
#endif
		protected override
#if NETSTANDARD2_0 || NET48_OR_GREATER
			async Task InstantiateReceivingAsync
#else
			void InstantiateReceiving
#endif
			(Manager manager, IDictionary<int, IMessageReceiver> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
#if NETSTANDARD2_0 || NET48_OR_GREATER
			await base.InstantiateReceivingAsync
#else
			base.InstantiateReceiving
#endif
				(manager, serviceBusReceivers, topicName, topicSubscriptionName);

#if NETSTANDARD2_0 || NET48_OR_GREATER
			// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/samples/Sample12_ManagingRules.md
			await using ServiceBusRuleManager ruleManager = (await GetOrCreateClientAsync()).CreateRuleManager(topicName, topicSubscriptionName);

			await lockObject.WaitAsync();
			try
			{
#else
			// https://docs.microsoft.com/en-us/azure/application-insights/app-insights-analytics-reference#summarize-operator
			// http://www.summa.com/blog/business-blog/everything-you-need-to-know-about-azure-service-bus-brokered-messaging-part-2#rulesfiltersactions
			// https://github.com/Azure-Samples/azure-servicebus-messaging-samples/tree/master/TopicFilters
			IMessageReceiver client;
			string connectionString = ConnectionString;
			AzureBusRbacSettings rbacSettings = RbacConnectionSettings;
			client = serviceBusReceivers[0];

			Task.Factory.StartNewSafely
			(() =>
			{
				lock (lockObject)
#endif
				{
					// Because refreshing the rule can take a while, we only want to do this when the value changes
					string filter;
					if (!ConfigurationManager.TryGetSetting(FilterKeyConfigurationKey, out filter))
						return;
					if (FilterKey.ContainsKey(topicName) && FilterKey[topicName] == filter)
						return;
					FilterKey[topicName] = filter;

#if NETSTANDARD2_0 || NET48_OR_GREATER
#else
					client = serviceBusReceivers[0];
#endif

					bool reAddRule = false;
					try
					{
#if NETSTANDARD2_0 || NET48_OR_GREATER
						IEnumerable<RuleDescription> rules = await ruleManager
							.GetRulesAsync()
							.Where(r => r.Name == "CqrsConfiguredFilter" || r.Name == "$Default")
							.ToListAsync();
#else
						IEnumerable<RuleDescription> rules = manager.GetRules(client.TopicPath, client.Name).ToList();
#endif
						RuleDescription ruleDescription = rules.SingleOrDefault(rule => rule.Name == "CqrsConfiguredFilter");
						if (ruleDescription != null)
						{
							var sqlFilter = ruleDescription.Filter as SqlFilter;
							if (sqlFilter == null && !string.IsNullOrWhiteSpace(filter))
								reAddRule = true;
							else if (sqlFilter != null && sqlFilter.SqlExpression != filter)
								reAddRule = true;
							if (sqlFilter != null && reAddRule)
							{
#if NETSTANDARD2_0 || NET48_OR_GREATER
								await ruleManager.DeleteRuleAsync(ruleDescription.Name);
#else
								client.RemoveRule(ruleDescription.Name);
#endif
							}
						}
						else if (!string.IsNullOrWhiteSpace(filter))
							reAddRule = true;

						ruleDescription = rules.SingleOrDefault(rule => rule.Name == "$Default");
						// If there is a default rule and we have a rule, it will cause issues
						if (!string.IsNullOrWhiteSpace(filter) && ruleDescription != null)
						{
#if NETSTANDARD2_0 || NET48_OR_GREATER
							await ruleManager.DeleteRuleAsync(ruleDescription.Name);
#else
							client.RemoveRule(ruleDescription.Name);
#endif
						}
						// If we don't have a rule and there is no longer a default rule, it will cause issues
						else if (string.IsNullOrWhiteSpace(filter) && !rules.Any())
						{
#if NETSTANDARD2_0 || NET48_OR_GREATER
							await ruleManager.CreateRuleAsync("$Default", new SqlFilter("1=1"));
#else
							ruleDescription = new RuleDescription
							(
								"$Default",
								new SqlFilter("1=1")
							);
							client.AddRule(ruleDescription);
#endif
						}
					}
					catch (AggregateException ex)
					{
						if (!(ex.InnerException is
#if NETSTANDARD2_0 || NET48_OR_GREATER
							ServiceBusException
#else
							MessagingEntityNotFoundException
#endif
						))
							throw;
					}
					catch
					(
#if NETSTANDARD2_0 || NET48_OR_GREATER
						ServiceBusException
#else
						MessagingEntityNotFoundException
#endif
					)
					{
					}

					if (!reAddRule)
						return;

					int loopCounter = 0;
					while (loopCounter < 10)
					{
						try
						{
#if NETSTANDARD2_0 || NET48_OR_GREATER
							await ruleManager.CreateRuleAsync("CqrsConfiguredFilter", new SqlFilter(filter));
#else
							RuleDescription ruleDescription = new RuleDescription
							(
								"CqrsConfiguredFilter",
								new SqlFilter(filter)
							);
							client.AddRule(ruleDescription);
#endif
							break;
						}
						catch
						(
#if NETSTANDARD2_0 || NET48_OR_GREATER
							ServiceBusException
#else
							MessagingEntityNotFoundException
#endif
							exception
						)
						{
							loopCounter++;
							// Still waiting for the delete to complete
							Thread.Sleep(1000);
							if (loopCounter == 9)
							{
								Logger.LogError("Setting the filter failed as it already exists.", exception: exception);
								TelemetryHelper.TrackException(exception);
							}
						}
						catch (Exception exception)
						{
							Logger.LogError("Setting the filter failed.", exception: exception);
							TelemetryHelper.TrackException(exception);
							break;
						}
					}
				}
			}
#if NETSTANDARD2_0 || NET48_OR_GREATER
			finally
			{
				await ruleManager.DisposeAsync();
				//When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
				//This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
				lockObject.Release();
			}
#else
			);
#endif
		}

		#endregion

		/// <summary>
		/// Register a command handler that will listen and respond to commands.
		/// </summary>
		/// <remarks>
		/// In many cases the <paramref name="targetedType"/> will be the handler class itself, what you actually want is the target of what is being updated.
		/// </remarks>
		public virtual
#if NETSTANDARD2_0 || NET48_OR_GREATER
			async Task RegisterHandlerAsync
#else
			void RegisterHandler
#endif
			<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
#if NETSTANDARD2_0 || NET48_OR_GREATER
			await AzureBusHelper.RegisterHandlerAsync
#else
			AzureBusHelper.RegisterHandler
#endif
			(TelemetryHelper, Routes, handler, targetedType, holdMessageLock);
		}

		/// <summary>
		/// Register a command handler that will listen and respond to commands.
		/// </summary>
		public virtual
#if NETSTANDARD2_0 || NET48_OR_GREATER
			async Task RegisterHandlerAsync
#else
			void RegisterHandler
#endif
			<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
#if NETSTANDARD2_0 || NET48_OR_GREATER
			await RegisterHandlerAsync
#else
			RegisterHandler
#endif
			(handler, null, holdMessageLock);
		}

#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Receives a <see cref="ProcessMessageEventArgs"/> from the command bus.
		/// </summary>
		public async virtual Task ReceiveCommandAsync(ProcessMessageEventArgs args)
		{
			FieldInfo[] fields = args.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.GetField);
			FieldInfo messageReceiverField = fields.SingleOrDefault(x => x.FieldType == typeof(ServiceBusReceiver));
			ServiceBusReceiver messageReceiver = (ServiceBusReceiver)messageReceiverField.GetValue(args);
			await ReceiveCommandAsync(messageReceiver, args.Message);
		}
#endif

		/// <summary>
		/// Receives a <see cref="BrokeredMessage"/> from the command bus.
		/// </summary>
		public virtual
#if NETSTANDARD2_0 || NET48_OR_GREATER
			async Task ReceiveCommandAsync(ServiceBusReceiver
#else
			void ReceiveCommand(IMessageReceiver
#endif
			 serviceBusReceiver, BrokeredMessage message)
		{
			DateTimeOffset startedAt = DateTimeOffset.UtcNow;
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			string responseCode = "200";
			// Null means it was skipped
			bool? wasSuccessfull = true;
			string telemetryName = $"Cqrs/Handle/Command/{message.MessageId}";
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
					Logger.LogDebug($"A command message arrived with the id '{message.MessageId}'.");
					string messageBody = message.GetBodyAsString();

					ICommand<TAuthenticationToken> command =
#if NETSTANDARD2_0 || NET48_OR_GREATER
						await AzureBusHelper.ReceiveCommandAsync(
#else
						AzureBusHelper.ReceiveCommand(
#endif
						serviceBusReceiver, messageBody,
#if NETSTANDARD2_0 || NET48_OR_GREATER
						ReceiveCommandAsync,
#else
						ReceiveCommand,
#endif
						$"id '{message.MessageId}'",
						ExtractSignature(message),
						SigningTokenConfigurationKey,
#if NETSTANDARD2_0 || NET48_OR_GREATER
						async
#endif
						() =>
						{
						wasSuccessfull = null;
							telemetryName = $"Cqrs/Handle/Command/Skipped/{message.MessageId}";
							responseCode = "204";
							// Remove message from queue
							try
							{
#if NETSTANDARD2_0 || NET48_OR_GREATER
								if (serviceBusReceiver != null)
									await serviceBusReceiver.CompleteMessageAsync(message);
#else
								message.Complete();
#endif
							}
							catch (AggregateException aggregateException)
							{
								if (aggregateException.InnerException is
#if NETSTANDARD2_0 || NET48_OR_GREATER
									ServiceBusException
#else
									MessageLockLostException
#endif
								)
#if NETSTANDARD2_0 || NET48_OR_GREATER
									throw aggregateException.InnerException;
#else
									throw new MessageLockLostException($"The lock supplied for the skipped command message '{message.MessageId}' is invalid.", aggregateException.InnerException);
#endif
								else
									throw;
							}
#if NETSTANDARD2_0 || NET48_OR_GREATER
#else
							catch (MessageLockLostException exception)
							{
								throw new MessageLockLostException($"The lock supplied for the skipped command message '{message.MessageId}' is invalid.", exception);
							}
#endif
							Logger.LogDebug($"A command message arrived with the id '{message.MessageId}' but processing was skipped due to event settings.");
							TelemetryHelper.TrackEvent("Cqrs/Handle/Command/Skipped", telemetryProperties);
						},
#if NETSTANDARD2_0 || NET48_OR_GREATER
						async
#endif
						() =>
						{
#if NETSTANDARD2_0 || NET48_OR_GREATER
							// Apparently locks are renewed automatically
							// see https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp#peeklock-behavior
							// ignore all of that and let's do this manually anyways

							await AzureBusHelper.RefreshLockAsync(serviceBusReceiver,
#else
							AzureBusHelper.RefreshLock(
#endif
								brokeredMessageRenewCancellationTokenSource, message, "command");
						}
					);

					if (wasSuccessfull != null)
					{
						if (command != null)
						{
							telemetryName = $"{command.GetType().FullName}/{command.Id}";
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

							telemetryName = $"Cqrs/Handle/Command/{telemetryName}";
						}
						// Remove message from queue
						try
						{
#if NETSTANDARD2_0 || NET48_OR_GREATER
							if (serviceBusReceiver != null)
								await serviceBusReceiver.CompleteMessageAsync(message);
#else
							message.Complete();
#endif
						}
						catch (AggregateException aggregateException)
						{
							if (aggregateException.InnerException is
#if NETSTANDARD2_0 || NET48_OR_GREATER
								ServiceBusException
#else
									MessageLockLostException
#endif
							)
#if NETSTANDARD2_0 || NET48_OR_GREATER
								throw aggregateException.InnerException;
#else
								throw new MessageLockLostException($"The lock supplied for command '{command.Id}' of type {command.GetType().Name} is invalid. To avoid this issue add a '{command.GetType().FullName}.ShouldRefresh' application settings", aggregateException.InnerException);
#endif
							else
								throw;
						}
#if NETSTANDARD2_0 || NET48_OR_GREATER
#else
						catch (MessageLockLostException exception)
						{
							throw new MessageLockLostException($"The lock supplied for command '{command.Id}' of type {command.GetType().Name} is invalid. To avoid this issue add a '{command.GetType().FullName}.ShouldRefresh' application settings", exception);
						}
#endif
					}
					Logger.LogDebug($"A command message arrived and was processed with the id '{message.MessageId}'.");
				}
				catch (AggregateException aggregateException)
				{
					throw aggregateException.InnerException;
				}
			}
			catch
			(
#if NETSTANDARD2_0 || NET48_OR_GREATER
				ServiceBusException
#else
				MessageLockLostException
#endif
				exception
			)
			{
				IDictionary<string, string> subTelemetryProperties = new Dictionary<string, string>(telemetryProperties);
				subTelemetryProperties.Add("TimeTaken", mainStopWatch.Elapsed.ToString());
				TelemetryHelper.TrackException(exception, null, subTelemetryProperties);
				if (ThrowExceptionOnReceiverMessageLockLostExceptionDuringComplete)
				{
					Logger.LogError(exception.Message, exception: exception);
					// Indicates a problem, unlock message in queue
					wasSuccessfull = false;
#if NETSTANDARD2_0 || NET48_OR_GREATER
					if (serviceBusReceiver != null)
						await serviceBusReceiver.AbandonMessageAsync(message);
					else
						throw;
#else
					message.Abandon();
#endif
				}
				else
				{
					Logger.LogWarning(exception.Message, exception: exception);
					try
					{
#if NETSTANDARD2_0 || NET48_OR_GREATER
						if (serviceBusReceiver != null)
							await serviceBusReceiver.DeadLetterMessageAsync(message, "LockLostButHandled", "The message was handled but the lock was lost.");
#else
						message.DeadLetter("LockLostButHandled", "The message was handled but the lock was lost.");
#endif
					}
					catch (Exception)
					{
						// Oh well, move on.
#if NETSTANDARD2_0 || NET48_OR_GREATER
						if (serviceBusReceiver != null)
							await serviceBusReceiver.AbandonMessageAsync(message);
						else
							throw;
#else
						message.Abandon();
#endif
					}
				}
				responseCode = "599";
			}
			catch (UnAuthorisedMessageReceivedException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError($"A command message arrived with the id '{message.MessageId}' but was not authorised.", exception: exception);
				wasSuccessfull = false;
				responseCode = "401";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
#if NETSTANDARD2_0 || NET48_OR_GREATER
				if (serviceBusReceiver != null)
					await serviceBusReceiver.DeadLetterMessageAsync(message, "UnAuthorisedMessageReceivedException", exception.Message);
				else
					throw;
#else
				message.DeadLetter("UnAuthorisedMessageReceivedException", exception.Message);
#endif
			}
			catch (NoHandlersRegisteredException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError($"A command message arrived with the id '{message.MessageId}' but no handlers were found to process it.", exception: exception);
				wasSuccessfull = false;
				responseCode = "501";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
#if NETSTANDARD2_0 || NET48_OR_GREATER
				if (serviceBusReceiver != null)
					await serviceBusReceiver.DeadLetterMessageAsync(message, "NoHandlersRegisteredException", exception.Message);
				else
					throw;
#else
				message.DeadLetter("NoHandlersRegisteredException", exception.Message);
#endif
			}
			catch (NoHandlerRegisteredException exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError($"A command message arrived with the id '{message.MessageId}' but no handler was found to process it.", exception: exception);
				wasSuccessfull = false;
				responseCode = "501";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
#if NETSTANDARD2_0 || NET48_OR_GREATER
				if (serviceBusReceiver != null)
					await serviceBusReceiver.DeadLetterMessageAsync(message, "NoHandlerRegisteredException", exception.Message);
				else
					throw;
#else
				message.DeadLetter("NoHandlerRegisteredException", exception.Message);
#endif
			}
			catch (Exception exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError($"A command message arrived with the id '{message.MessageId}' but failed to be process.", exception: exception);
				wasSuccessfull = false;
				responseCode = "500";
				telemetryProperties.Add("ExceptionType", exception.GetType().FullName);
				telemetryProperties.Add("ExceptionMessage", exception.Message);
#if NETSTANDARD2_0 || NET48_OR_GREATER
				if (serviceBusReceiver != null)
					await serviceBusReceiver.AbandonMessageAsync(message);
				else
					throw;
#else
				message.Abandon();
#endif
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
						guidAuthenticationToken ,
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
		public virtual
#if NETSTANDARD2_0 || NET48_OR_GREATER
			async Task<bool?> ReceiveCommandAsync
#else
			bool? ReceiveCommand
#endif
			(ICommand<TAuthenticationToken> command)
		{
			return
#if NETSTANDARD2_0 || NET48_OR_GREATER
				await AzureBusHelper.DefaultReceiveCommandAsync
#else
				AzureBusHelper.DefaultReceiveCommand
#endif
			(command, Routes, "Azure-ServiceBus");
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
		public virtual void Start()
		{
#if NETSTANDARD2_0 || NET48_OR_GREATER
			SafeTask.RunSafelyAsync(async () => {
				await InstantiateReceivingAsync();

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

				await RegisterReceiverMessageHandlerAsync(ReceiveCommandAsync, options);
			}).Wait();
#else
			InstantiateReceiving();
			var options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
				// I think this is intentionally left out
				// , MaxConcurrentCalls = MaximumConcurrentReceiverProcessesCount
			};

			// Callback to handle received messages
			RegisterReceiverMessageHandler(ReceiveCommand, options);
#endif
		}

		#endregion
	}
}