#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Configuration;
#if NET452
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Manager = Microsoft.ServiceBus.NamespaceManager;
#endif
#if NETSTANDARD2_0
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Manager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
#endif
using System.Text;
using Cqrs.Bus;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// An <see cref="AzureBus{TAuthenticationToken}"/> that uses Azure Service Event Hubs.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class AzureEventHub<TAuthenticationToken>
		: AzureBus<TAuthenticationToken>
	{
		/// <summary>
		/// Gets the public<see cref="EventHubClient"/>.
		/// </summary>
		protected EventHubClient EventHubPublisher { get; private set; }

		/// <summary>
		/// Gets the public<see cref="EventProcessorHost"/>.
		/// </summary>
		protected EventProcessorHost EventHubReceiver { get; private set; }

		/// <summary>
		/// The name of the private event hub.
		/// </summary>
		protected string PrivateEventHubName { get; set; }

		/// <summary>
		/// The name of the public event hub.
		/// </summary>
		protected string PublicEventHubName { get; private set; }

		/// <summary>
		/// The name of the consumer group in the private event hub.
		/// </summary>
		protected string PrivateEventHubConsumerGroupName { get; private set; }

		/// <summary>
		/// The name of the consumer group in the public event hub.
		/// </summary>
		protected string PublicEventHubConsumerGroupName { get; private set; }

		/// <summary>
		/// The configuration key for the event hub connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string EventHubConnectionStringNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the event hub storage connection string as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string EventHubStorageConnectionStringNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string SigningTokenConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the private event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PrivateEventHubNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the public event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PublicEventHubNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the consumer group name of the private event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PrivateEventHubConsumerGroupNameConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the name of the consumer group name of the public event hub as used by <see cref="IConfigurationManager"/>.
		/// </summary>
		protected abstract string PublicEventHubConsumerGroupNameConfigurationKey { get; }

		/// <summary>
		/// The default name of the private event hub if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected abstract string DefaultPrivateEventHubName { get; }

		/// <summary>
		/// The default name of the public event hub if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected abstract string DefaultPublicEventHubName { get; }

		/// <summary>
		/// The default name of the consumer group in the private event hub if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected const string DefaultPrivateEventHubConsumerGroupName = "$Default";

		/// <summary>
		/// The default name of the consumer group in the public event hub if no <see cref="IConfigurationManager"/> value is set.
		/// </summary>
		protected const string DefaultPublicEventHubConsumerGroupName = "$Default";

		/// <summary>
		/// The event hub storage connection string.
		/// </summary>
		protected string StorageConnectionString { get; private set; }

		/// <summary>
		/// The <see cref="Action{PartitionContext, EventData}">handler</see> used for <see cref="EventProcessorHost.RegisterEventProcessorFactoryAsync(IEventProcessorFactory)"/> on <see cref="EventHubReceiver"/>.
		/// </summary>
		protected Action<PartitionContext, EventData> ReceiverMessageHandler { get; private set; }

		/// <summary>
		/// The <see cref="EventProcessorOptions" /> used for <see cref="EventProcessorHost.RegisterEventProcessorFactoryAsync(IEventProcessorFactory)"/> on <see cref="EventHubReceiver"/>.
		/// </summary>
		protected EventProcessorOptions ReceiverMessageHandlerOptions { get; private set; }

		/// <summary>
		/// Gets the <see cref="ITelemetryHelper"/>.
		/// </summary>
		protected ITelemetryHelper TelemetryHelper { get; set; }

		/// <summary>
		/// The <see cref="IHashAlgorithmFactory"/> to use to sign messages.
		/// </summary>
		protected IHashAlgorithmFactory Signer { get; private set; }

		/// <summary>
		/// A list of namespaces to exclude when trying to automatically determine the container.
		/// </summary>
		protected IList<string> ExclusionNamespaces { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureEventHub{TAuthenticationToken}"/>
		/// </summary>
		protected AzureEventHub(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IHashAlgorithmFactory hashAlgorithmFactory, bool isAPublisher)
			: base (configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
			TelemetryHelper = new NullTelemetryHelper();
			ExclusionNamespaces = new SynchronizedCollection<string> { "Cqrs", "System" };
			Signer = hashAlgorithmFactory;
		}

#region Overrides of AzureBus<TAuthenticationToken>

		/// <summary>
		/// Gets the connection string for the bus from <see cref="AzureBus{TAuthenticationToken}.ConfigurationManager"/>
		/// </summary>
		protected override string GetConnectionString()
		{
			string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(EventHubConnectionStringNameConfigurationKey)].ConnectionString;
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ConfigurationErrorsException(string.Format("Configuration is missing required information. Make sure the appSetting '{0}' is defined and a matching connection string with the name that matches the value of the appSetting value '{0}'.", EventHubConnectionStringNameConfigurationKey));
			return connectionString;
		}

		/// <summary>
		/// Calls <see cref="AzureBus{TAuthenticationToken}.SetConnectionStrings"/>
		/// and then sets the required storage connection string.
		/// </summary>
		protected override void SetConnectionStrings()
		{
			base.SetConnectionStrings();
			StorageConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigurationManager.GetSetting(EventHubStorageConnectionStringNameConfigurationKey)].ConnectionString;
			if (string.IsNullOrWhiteSpace(StorageConnectionString))
				throw new ConfigurationErrorsException(string.Format("Configuration is missing required information. Make sure the appSetting '{0}' is defined and a matching connection string with the name that matches the value of the appSetting value '{0}'.", EventHubStorageConnectionStringNameConfigurationKey));
			Logger.LogDebug(string.Format("Storage connection string settings set to {0}.", StorageConnectionString));
		}

#endregion

		/// <summary>
		/// Instantiate publishing on this bus by
		/// calling <see cref="CheckPrivateHubExists"/> and <see cref="CheckPublicHubExists"/>
		/// then calling <see cref="AzureBus{TAuthenticationToken}.StartSettingsChecking"/>
		/// </summary>
		protected override void InstantiatePublishing()
		{
#if NET452
			Manager manager = Manager.CreateFromConnectionString(ConnectionString);
#endif
#if NETSTANDARD2_0
			var manager = new Manager(ConnectionString);
#endif
			CheckPrivateHubExists(manager);
			CheckPublicHubExists(manager);

#if NET452
			EventHubPublisher = EventHubClient.CreateFromConnectionString(ConnectionString, PublicEventHubName);
#endif
#if NETSTANDARD2_0
			var connectionStringBuilder = new EventHubsConnectionStringBuilder(ConnectionString)
			{
				EntityPath = PublicEventHubName
			};
			EventHubPublisher = EventHubClient.Create(connectionStringBuilder);
#endif
			StartSettingsChecking();
		}

		/// <summary>
		/// Instantiate receiving on this bus by
		/// calling <see cref="CheckPrivateHubExists"/> and <see cref="CheckPublicHubExists"/>
		/// then InstantiateReceiving for private and public topics,
		/// then calling <see cref="AzureBus{TAuthenticationToken}.StartSettingsChecking"/>
		/// </summary>
		protected override void InstantiateReceiving()
		{
#if NET452
			Manager manager = Manager.CreateFromConnectionString(ConnectionString);
#endif
#if NETSTANDARD2_0
			var manager = new Manager(ConnectionString);
#endif

			CheckPrivateHubExists(manager);
			CheckPublicHubExists(manager);

			EventHubReceiver = new EventProcessorHost(PublicEventHubName, PublicEventHubConsumerGroupName, ConnectionString, StorageConnectionString, "Cqrs");

			// If this is also a publisher, then it will the check over there and that will handle this
			if (EventHubPublisher != null)
				return;

			StartSettingsChecking();
		}

		/// <summary>
		/// Checks if the private hub and consumer group name exists as per <see cref="PrivateEventHubName"/> and <see cref="PrivateEventHubConsumerGroupName"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		protected virtual void CheckPrivateHubExists(Manager manager)
		{
			CheckHubExists(manager, PrivateEventHubName = ConfigurationManager.GetSetting(PrivateEventHubNameConfigurationKey) ?? DefaultPrivateEventHubName, PrivateEventHubConsumerGroupName = ConfigurationManager.GetSetting(PrivateEventHubConsumerGroupNameConfigurationKey) ?? DefaultPrivateEventHubConsumerGroupName);
		}

		/// <summary>
		/// Checks if the public hub and consumer group name exists as per <see cref="PublicEventHubName"/> and <see cref="PublicEventHubConsumerGroupName"/>.
		/// </summary>
		/// <param name="manager">The <see cref="Manager"/>.</param>
		protected virtual void CheckPublicHubExists(Manager manager)
		{
			CheckHubExists(manager, PublicEventHubName = ConfigurationManager.GetSetting(PublicEventHubNameConfigurationKey) ?? DefaultPublicEventHubName, PublicEventHubConsumerGroupName = ConfigurationManager.GetSetting(PublicEventHubConsumerGroupNameConfigurationKey) ?? DefaultPublicEventHubConsumerGroupName);
		}

		/// <summary>
		/// Checks if a event hub by the provided <paramref name="hubName"/> exists and
		/// Checks if a consumer group by the provided <paramref name="consumerGroupNames"/> exists.
		/// </summary>
		protected virtual void CheckHubExists(Manager manager, string hubName, string consumerGroupNames)
		{
#if NET452
			// Configure Queue Settings
			var eventHubDescription = new EventHubDescription(hubName)
			{
				MessageRetentionInDays = long.MaxValue,
			};

			// Create the topic if it does not exist already
			manager.CreateEventHubIfNotExists(eventHubDescription);

			var subscriptionDescription = new SubscriptionDescription(eventHubDescription.Path, consumerGroupNames);

			if (!manager.SubscriptionExists(eventHubDescription.Path, consumerGroupNames))
				manager.CreateSubscription(subscriptionDescription);
#endif
#if NETSTANDARD2_0
			/*
			// Configure Queue Settings
			var eventHubDescription = new EventHubDescription(hubName)
			{
				MessageRetentionInDays = long.MaxValue,
			};

			// Create the topic if it does not exist already
			manager.CreateEventHubIfNotExists(eventHubDescription);

			Task<bool> checkTask = manager.SubscriptionExistsAsync(eventHubDescription.Path, consumerGroupNames);
			checkTask.Wait(1500);
			if (!checkTask.Result)
				manager.CreateSubscriptionAsync(subscriptionDescription).Wait(1500);
			*/
			Logger.LogWarning($"Checking EventHubs and subscriptions is not currently implemented until the Azure libraries provide management facilities. You will need to check these objects exist manually: EventHub {hubName}, Subscription/Consumer Group {consumerGroupNames}", "AzureEventHub");
#endif
		}

		/// <summary>
		/// Checks <see cref="AzureBus{TAuthenticationToken}.ValidateSettingsHaveChanged"/>
		/// and that <see cref="StorageConnectionString"/> have changed.
		/// </summary>
		/// <returns></returns>
		protected override bool ValidateSettingsHaveChanged()
		{
			return base.ValidateSettingsHaveChanged()
				||
			StorageConnectionString != ConfigurationManager.GetSetting(EventHubStorageConnectionStringNameConfigurationKey);
		}

		/// <summary>
		/// Triggers settings checking on <see cref="EventHubPublisher"/> and <see cref="EventHubReceiver"/>,
		/// then calls <see cref="InstantiateReceiving"/> and <see cref="InstantiatePublishing"/>.
		/// </summary>
		protected override void TriggerSettingsChecking()
		{
			// Let's wrap up using this event hub and start the switch
			if (EventHubPublisher != null)
			{
				EventHubPublisher.Close();
				Logger.LogDebug("Publishing event hub closed.");
			}
			// Let's wrap up using this event hub and start the switch
			if (EventHubReceiver != null)
			{
				Task work = EventHubReceiver.UnregisterEventProcessorAsync();
				work.ConfigureAwait(false);
				work.Wait();
				Logger.LogDebug("Receiving event hub closed.");
			}
			// Restart configuration, we order this intentionally with the receiver first as if this triggers the cancellation we know this isn't a publisher as well
			if (EventHubReceiver != null)
			{
				Logger.LogDebug("Recursively calling into InstantiateReceiving.");
				InstantiateReceiving();

				// This will be the case of a connection setting change re-connection
				if (ReceiverMessageHandler != null && ReceiverMessageHandlerOptions != null)
				{
					// Callback to handle received messages
					Logger.LogDebug("Re-registering onMessage handler.");
					ApplyReceiverMessageHandler();
				}
				else
					Logger.LogWarning("No onMessage handler was found to re-bind.");
			}
			// Restart configuration, we order this intentionally with the publisher second as if this triggers the cancellation there's nothing else to process here
			if (EventHubPublisher != null)
			{
				Logger.LogDebug("Recursively calling into InstantiatePublishing.");
				InstantiatePublishing();
			}
		}

		/// <summary>
		/// Registers the provided <paramref name="receiverMessageHandler"/> with the provided <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
		protected virtual void RegisterReceiverMessageHandler(Action<PartitionContext, EventData> receiverMessageHandler, EventProcessorOptions receiverMessageHandlerOptions = null)
		{
			StoreReceiverMessageHandler(receiverMessageHandler, receiverMessageHandlerOptions);

			ApplyReceiverMessageHandler();
		}

		/// <summary>
		/// Stores the provided <paramref name="receiverMessageHandler"/> and <paramref name="receiverMessageHandlerOptions"/>.
		/// </summary>
		protected virtual void StoreReceiverMessageHandler(Action<PartitionContext, EventData> receiverMessageHandler, EventProcessorOptions receiverMessageHandlerOptions = null)
		{
			ReceiverMessageHandler = receiverMessageHandler;
			ReceiverMessageHandlerOptions = receiverMessageHandlerOptions;
		}

		/// <summary>
		/// Applies the stored ReceiverMessageHandler and ReceiverMessageHandlerOptions to the <see cref="EventHubReceiver"/>.
		/// </summary>
		protected override void ApplyReceiverMessageHandler()
		{
			EventHubReceiver.RegisterEventProcessorFactoryAsync
			(
				new DefaultEventProcessorFactory<DefaultEventProcessor>
				(
					new DefaultEventProcessor(Logger, ReceiverMessageHandler)
				),
				ReceiverMessageHandlerOptions ?? EventProcessorOptions.DefaultOptions
			);
		}

		/// <summary>
		/// Create <see cref="EventData"/> with additional properties to aid routing and tracing
		/// </summary>
		protected virtual EventData CreateBrokeredMessage<TMessage>(Func<TMessage, string> serialiserFunction, Type messageType, TMessage message)
		{
			string messageBody = serialiserFunction(message);
			var brokeredMessage = new EventData(Encoding.UTF8.GetBytes(messageBody));

			brokeredMessage.Properties.Add("CorrelationId", CorrelationIdHelper.GetCorrelationId().ToString("N"));
			brokeredMessage.Properties.Add("Type", messageType.FullName);
			brokeredMessage.Properties.Add("Source", string.Format("{0}/{1}/{2}/{3}", Logger.LoggerSettings.ModuleName, Logger.LoggerSettings.Instance, Logger.LoggerSettings.Environment, Logger.LoggerSettings.EnvironmentInstance));

			// see https://github.com/Chinchilla-Software-Com/CQRS/wiki/Inter-process-function-security</remarks>
			string configurationKey = string.Format("{0}.SigningToken", messageType.FullName);
			string signingToken;
			HashAlgorithm signer = Signer.Create();
			if (!ConfigurationManager.TryGetSetting(configurationKey, out signingToken) || string.IsNullOrWhiteSpace(signingToken))
				if (!ConfigurationManager.TryGetSetting(SigningTokenConfigurationKey, out signingToken) || string.IsNullOrWhiteSpace(signingToken))
					signingToken = Guid.Empty.ToString("N");
			if (!string.IsNullOrWhiteSpace(signingToken))
				using (var hashStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Concat("{0}{1}", signingToken, messageBody))))
					brokeredMessage.Properties.Add("Signature", Convert.ToBase64String(signer.ComputeHash(hashStream)));

			try
			{
				var stackTrace = new StackTrace();
				StackFrame[] stackFrames = stackTrace.GetFrames();
				if (stackFrames != null)
				{
					foreach (StackFrame frame in stackFrames)
					{
						MethodBase method = frame.GetMethod();
						if (method.ReflectedType == null)
							continue;

						try
						{
							if (ExclusionNamespaces.All(@namespace => !method.ReflectedType.FullName.StartsWith(@namespace)))
							{
								brokeredMessage.Properties.Add("Source-Method", string.Format("{0}.{1}", method.ReflectedType.FullName, method.Name));
								break;
							}
						}
						catch
						{
							// Just move on
						}
					}
				}
			}
			catch
			{
				// Just move on
			}

			return brokeredMessage;
		}

		/// <summary>
		/// Extract any telemetry properties from the provided <paramref name="message"/>.
		/// </summary>
		protected virtual IDictionary<string, string> ExtractTelemetryProperties(EventData message, string baseCommunicationType)
		{
			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", baseCommunicationType } };
			object value;
			if (message.Properties.TryGetValue("Type", out value))
				telemetryProperties.Add("MessageType", value.ToString());
			if (message.Properties.TryGetValue("Source", out value))
				telemetryProperties.Add("MessageSource", value.ToString());
			if (message.Properties.TryGetValue("Source-Method", out value))
				telemetryProperties.Add("MessageSourceMethod", value.ToString());
			if (message.Properties.TryGetValue("CorrelationId", out value) && !telemetryProperties.ContainsKey("CorrelationId"))
				telemetryProperties.Add("CorrelationId", value.ToString());

			return telemetryProperties;
		}

		/// <summary>
		/// Extract the signature from the provided <paramref name="eventData"/>.
		/// </summary>
		protected virtual string ExtractSignature(EventData eventData)
		{
			object value;
			if (eventData.Properties.TryGetValue("Signature", out value))
				return value.ToString();
			return null;
		}
	}
}