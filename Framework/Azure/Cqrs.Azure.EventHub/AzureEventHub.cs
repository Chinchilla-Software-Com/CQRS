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
using System.Text;
using System.Threading.Tasks;

using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;

#if NETSTANDARD2_0
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Manager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
using TokenProvider = Microsoft.Azure.ServiceBus.Primitives.TokenProvider;
using AzureActiveDirectoryTokenProvider2 = Microsoft.Azure.ServiceBus.Primitives.AzureActiveDirectoryTokenProvider;
#else
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Manager = Microsoft.ServiceBus.NamespaceManager;
#endif

#if NET462
using AzureActiveDirectoryTokenProvider2 = Microsoft.ServiceBus.AzureActiveDirectoryTokenProvider;
#endif

#if NET452
#else
using Microsoft.Identity.Client;
#endif

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
		/// The configuration key for the event hub connection endpoint as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected abstract string EventHubConnectionEndpointConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the event hub connection Application Id as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected abstract string EventHubConnectionApplicationIdConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the event hub connection Client Key/Secret as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected abstract string EventHubConnectionClientKeyConfigurationKey { get; }

		/// <summary>
		/// The configuration key for the event hub connection Tenant Id as used by <see cref="IConfigurationManager"/>, when using RBAC.
		/// </summary>
		protected abstract string EventHubConnectionTenantIdConfigurationKey { get; }

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

#if NET452
#else
		/// <summary>
		/// Gets an access token from Active Directory when using RBAC based connections.
		/// </summary>
		protected AzureActiveDirectoryTokenProvider2.AuthenticationCallback GetActiveDirectoryToken { get; private set; }

		/// <summary>
		/// Gets an access token from Active Directory when using RBAC based connections.
		/// </summary>
		protected AzureActiveDirectoryTokenProvider.AuthenticationCallback GetActiveDirectoryToken2 { get; private set; }
#endif

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureEventHub{TAuthenticationToken}"/>
		/// </summary>
		protected AzureEventHub(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IHashAlgorithmFactory hashAlgorithmFactory, bool isAPublisher)
			: base (configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
			TelemetryHelper = new NullTelemetryHelper();
			ExclusionNamespaces = new SynchronizedCollection<string> { "Cqrs", "System" };
			Signer = hashAlgorithmFactory;

			Instantiate();
		}

		private void Instantiate()
		{
#if NET452
#else
			GetActiveDirectoryToken = async (audience, authority, state) =>
			{
				string applicationId = ConfigurationManager.GetSetting(EventHubConnectionApplicationIdConfigurationKey);
				string clientKey = ConfigurationManager.GetSetting(EventHubConnectionClientKeyConfigurationKey);

				IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(applicationId)
					.WithAuthority(authority)
					.WithClientSecret(clientKey)
					.Build();

				var authResult = await app
					.AcquireTokenForClient(new string[] { "https://servicebus.azure.net/.default" })
					.ExecuteAsync();

				return authResult.AccessToken;
			};

			GetActiveDirectoryToken2 = async (audience, authority, state) =>
			{
				string applicationId = ConfigurationManager.GetSetting(EventHubConnectionApplicationIdConfigurationKey);
				string clientKey = ConfigurationManager.GetSetting(EventHubConnectionClientKeyConfigurationKey);

				IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(applicationId)
					.WithAuthority(authority)
					.WithClientSecret(clientKey)
					.Build();

				var authResult = await app
					.AcquireTokenForClient(new string[] { "https://servicebus.azure.net/.default" })
					.ExecuteAsync();

				return authResult.AccessToken;
			};
#endif
		}

		#region Overrides of AzureBus<TAuthenticationToken>

		/// <summary>
		/// Gets the connection string for the bus from <see cref="AzureBus{TAuthenticationToken}.ConfigurationManager"/>
		/// </summary>
		protected override string GetConnectionString()
		{
			string connectionStringKey = ConfigurationManager.GetSetting(EventHubConnectionStringNameConfigurationKey);
			if (string.IsNullOrWhiteSpace(connectionStringKey))
			{
				string connectionEndpoint = ConfigurationManager.GetSetting(EventHubConnectionEndpointConfigurationKey);
				// double check an endpoint isn't provided, if it is, then we're using endpoints, but if not, we'll assume a connection string is prefered as it's easier
				if (string.IsNullOrWhiteSpace(connectionEndpoint))
					throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{EventHubConnectionStringNameConfigurationKey}' is defined and has a valid connection string with the name that matches the value of the appSetting value '{EventHubConnectionStringNameConfigurationKey}'.");
			}
			string connectionString = ConfigurationManager.GetConnectionStringBySettingKey(connectionStringKey, true);

			return connectionString;
		}

		/// <summary>
		/// Gets the RBAC connection settings for the bus from <see cref="AzureBus{TAuthenticationToken}.ConfigurationManager"/>
		/// </summary>
		protected override AzureBusRbacSettings GetRbacConnectionSettings()
		{
			// double check an endpoint isn't provided, if it is, then we're using endpoints, but if not, we'll assume a connection string is prefered as it's easier
			bool isUsingConnectionString = !string.IsNullOrWhiteSpace(ConfigurationManager.GetSetting(EventHubConnectionEndpointConfigurationKey));

			string endpoint = ConfigurationManager.GetSetting(EventHubConnectionEndpointConfigurationKey);
			if (!isUsingConnectionString && string.IsNullOrWhiteSpace(endpoint))
				throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{EventHubConnectionEndpointConfigurationKey}' is defined and has a valid connection endpoint value.");

			string applicationId = ConfigurationManager.GetSetting(EventHubConnectionApplicationIdConfigurationKey);
			if (!isUsingConnectionString && string.IsNullOrWhiteSpace(applicationId))
				throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{EventHubConnectionApplicationIdConfigurationKey}' is defined and has a valid application id value.");

			string clientKey = ConfigurationManager.GetSetting(EventHubConnectionClientKeyConfigurationKey);
			if (!isUsingConnectionString && string.IsNullOrWhiteSpace(clientKey))
				throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{EventHubConnectionClientKeyConfigurationKey}' is defined and has a valid client key/secret value.");

			string tenantId = ConfigurationManager.GetSetting(EventHubConnectionTenantIdConfigurationKey);
			if (!isUsingConnectionString && string.IsNullOrWhiteSpace(tenantId))
				throw new ConfigurationErrorsException($"Configuration is missing required information. Make sure the appSetting '{EventHubConnectionTenantIdConfigurationKey}' is defined and has a valid tenant id value.");

			return new AzureBusRbacSettings
			{
				Endpoint = endpoint,
				ApplicationId = applicationId,
				ClientKey = clientKey,
				TenantId = tenantId
			};
		}

		/// <summary>
		/// Calls <see cref="AzureBus{TAuthenticationToken}.SetConnectionStrings"/>
		/// and then sets the required storage connection string.
		/// </summary>
		protected override void SetConnectionStrings()
		{
			base.SetConnectionStrings();
			string connectionStringKey = ConfigurationManager.GetSetting(EventHubStorageConnectionStringNameConfigurationKey);
			StorageConnectionString = ConfigurationManager.GetConnectionStringBySettingKey(connectionStringKey, true);
			Logger.LogSensitive($"Storage connection string settings set to '{StorageConnectionString}'.");
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
#else
			if (GetActiveDirectoryToken == null)
				Instantiate();
#endif

			Manager manager;
			string connectionString = ConnectionString;
			AzureBusRbacSettings rbacSettings = RbacConnectionSettings;
#if NETSTANDARD2_0
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = new Manager(ConnectionString);
			else
				manager = new Manager(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority()));
#else
#if NET452
			manager = Manager.CreateFromConnectionString(ConnectionString);
#else
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = Manager.CreateFromConnectionString(ConnectionString);
			else
				manager = new Manager(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, new Uri(rbacSettings.Endpoint), rbacSettings.GetDefaultAuthority()));
#endif
#endif
			CheckPrivateHubExists(manager);
			CheckPublicHubExists(manager);

#if NETSTANDARD2_0
			if (!string.IsNullOrWhiteSpace(connectionString))
				EventHubPublisher = EventHubClient.CreateFromConnectionString(connectionString);
			else
				EventHubPublisher = EventHubClient.CreateWithAzureActiveDirectory(new Uri(rbacSettings.Endpoint), PublicEventHubName, GetActiveDirectoryToken2, rbacSettings.GetDefaultAuthority());
#else
#if NET452
			EventHubPublisher = EventHubClient.CreateFromConnectionString(connectionString);
#else
			if (!string.IsNullOrWhiteSpace(connectionString))
				EventHubPublisher = EventHubClient.CreateFromConnectionString(connectionString);
			else
				EventHubPublisher = EventHubClient.CreateWithAzureActiveDirectory(new Uri(rbacSettings.Endpoint), PublicEventHubName, GetActiveDirectoryToken2, rbacSettings.GetDefaultAuthority());
#endif
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
#else
			if (GetActiveDirectoryToken == null)
				Instantiate();
#endif

			Manager manager;
			string connectionString = ConnectionString;
			AzureBusRbacSettings rbacSettings = RbacConnectionSettings;
#if NETSTANDARD2_0
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = new Manager(ConnectionString);
			else
				manager = new Manager(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, rbacSettings.GetDefaultAuthority()));
#else
#if NET452
			manager = Manager.CreateFromConnectionString(ConnectionString);
#else
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = Manager.CreateFromConnectionString(ConnectionString);
			else
				manager = new Manager(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, new Uri(rbacSettings.Endpoint), rbacSettings.GetDefaultAuthority()));
#endif
#endif

			CheckPrivateHubExists(manager);
			CheckPublicHubExists(manager);

#if NETSTANDARD2_0
			if (!string.IsNullOrWhiteSpace(connectionString))
				EventHubReceiver = new EventProcessorHost(PublicEventHubName, PublicEventHubConsumerGroupName, ConnectionString, StorageConnectionString, "Cqrs");
			else
				EventHubReceiver = new EventProcessorHost(new Uri(rbacSettings.Endpoint), PublicEventHubName, PublicEventHubConsumerGroupName, Microsoft.Azure.EventHubs.TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken2, rbacSettings.Endpoint, rbacSettings.GetDefaultAuthority()), Microsoft.Azure.Storage.CloudStorageAccount.Parse(StorageConnectionString), "Cqrs");
#else
#if NET452
			EventHubReceiver = new EventProcessorHost(PublicEventHubName, PublicEventHubConsumerGroupName, ConnectionString, StorageConnectionString, "Cqrs");
#else
			if (!string.IsNullOrWhiteSpace(connectionString))
				EventHubReceiver = new EventProcessorHost(PublicEventHubName, PublicEventHubConsumerGroupName, ConnectionString, StorageConnectionString, "Cqrs");
			else
			{
				Func<EventProcessorOptions, MessagingFactory> eventHubClientFactory = (options) => { return MessagingFactory.Create(rbacSettings.Endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetActiveDirectoryToken, new Uri(rbacSettings.Endpoint), rbacSettings.GetDefaultAuthority())); };
				EventHubReceiver = new EventProcessorHost(rbacSettings.Endpoint, PublicEventHubName, PublicEventHubConsumerGroupName, eventHubClientFactory, () => { return new Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient(new Uri(StorageConnectionString)); }, "Cqrs");
			}
#endif
#endif

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
#else
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

			brokeredMessage.AddUserProperty("CorrelationId", CorrelationIdHelper.GetCorrelationId().ToString("N"));
			brokeredMessage.AddUserProperty("Type", messageType.FullName);
			brokeredMessage.AddUserProperty("Source", string.Format("{0}/{1}/{2}/{3}", Logger.LoggerSettings.ModuleName, Logger.LoggerSettings.Instance, Logger.LoggerSettings.Environment, Logger.LoggerSettings.EnvironmentInstance));

			// see https://github.com/Chinchilla-Software-Com/CQRS/wiki/Inter-process-function-security</remarks>
			string configurationKey = string.Format("{0}.SigningToken", messageType.FullName);
			string signingToken;
			HashAlgorithm signer = Signer.Create();
			if (!ConfigurationManager.TryGetSetting(configurationKey, out signingToken) || string.IsNullOrWhiteSpace(signingToken))
				if (!ConfigurationManager.TryGetSetting(SigningTokenConfigurationKey, out signingToken) || string.IsNullOrWhiteSpace(signingToken))
					signingToken = Guid.Empty.ToString("N");
			if (!string.IsNullOrWhiteSpace(signingToken))
				using (var hashStream = new MemoryStream(Encoding.UTF8.GetBytes($"{signingToken}{messageBody}")))
					brokeredMessage.AddUserProperty("Signature", Convert.ToBase64String(signer.ComputeHash(hashStream)));

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
								brokeredMessage.AddUserProperty("Source-Method", string.Format("{0}.{1}", method.ReflectedType.FullName, method.Name));
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
			if (message.TryGetUserPropertyValue("Type", out value))
				telemetryProperties.Add("MessageType", value.ToString());
			if (message.TryGetUserPropertyValue("Source", out value))
				telemetryProperties.Add("MessageSource", value.ToString());
			if (message.TryGetUserPropertyValue("Source-Method", out value))
				telemetryProperties.Add("MessageSourceMethod", value.ToString());
			if (message.TryGetUserPropertyValue("CorrelationId", out value) && !telemetryProperties.ContainsKey("CorrelationId"))
				telemetryProperties.Add("CorrelationId", value.ToString());

			return telemetryProperties;
		}

		/// <summary>
		/// Extract the signature from the provided <paramref name="eventData"/>.
		/// </summary>
		protected virtual string ExtractSignature(EventData eventData)
		{
			object value;
			if (eventData.TryGetUserPropertyValue("Signature", out value))
				return value.ToString();
			return null;
		}
	}
}