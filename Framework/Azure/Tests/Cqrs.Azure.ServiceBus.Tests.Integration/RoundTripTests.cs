using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using NUnit.Framework;

#if NET472
using Cqrs.Azure.ConfigurationManager;
using Manager = Microsoft.ServiceBus.NamespaceManager;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
using Microsoft.Identity.Client;
using Microsoft.ServiceBus;
#else
using Azure.Identity;
using BrokeredMessage = Azure.Messaging.ServiceBus.ServiceBusReceivedMessage;
using Manager = Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient;
using TopicClient = Azure.Messaging.ServiceBus.ServiceBusSender;
using TopicDescription = Azure.Messaging.ServiceBus.Administration.CreateTopicOptions;
#endif

#if NET472_OR_GREATER
#else
using Cqrs.Azure.ConfigurationManager;
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Azure.ServiceBus.Tests.Integration
{
	/// <summary>
	/// A series of tests publishing <see cref="IEvent{TAuthenticationToken}">events</see>.
	/// </summary>
	[TestClass]
	public class RoundTripTests
	{
		/// <summary>
		/// Tests a test <see cref="IEvent{TAuthenticationToken}"/> can be published via
		/// <see cref="AzureEventBusPublisher{TAuthenticationToken}"/> and two <see cref="IEventHandler">event handlers</see>
		/// Will fire updating test flags.
		/// </summary>
		[TestMethod]
		public
#if NET472
			void
#else
			async Task
#endif
			Publish_TestEvent_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var @event = new TestEvent{Id = processId};
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
			DependencyResolver.ConfigurationManager = configurationManager;
#endif

			var azureEventBusReceiver = new AzureEventBusReceiver<Guid>(configurationManager, new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), configurationManager, null), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory());
			var handler = new TestEventSuccessHandler(testResponse);
#if NET472
			azureEventBusReceiver.RegisterHandler
#else
			await azureEventBusReceiver.RegisterHandlerAsync
#endif
				<TestEvent>(handler.
#if NET472
					Handle
#else
					HandleAsync
#endif
				, handler.GetType());
			azureEventBusReceiver.Start();

			var azureEventBusPublisher = new AzureEventBusPublisher<Guid>(configurationManager, new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), configurationManager, null), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory());

			// Act
#if NET472
			azureEventBusPublisher.Publish
#else
			await azureEventBusPublisher.PublishAsync
#endif
				(@event);

			// Assert
			SpinWait.SpinUntil(() => testResponse[processId].Item1);
			Assert.IsNull(testResponse[processId].Item2);
		}

		/// <summary>
		/// Tests a test <see cref="ICommand{TAuthenticationToken}"/> can be published via
		/// <see cref="AzureCommandBusPublisher{TAuthenticationToken}"/> and two <see cref="IEventHandler">event handlers</see>
		/// Will fire updating test flags.
		/// </summary>
		[TestMethod]
		public
#if NET472
			void
#else
			async Task
#endif
			Publish_TestCommand_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var command = new TestCommand { Id = processId };
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
			Configuration.ConfigurationManager.BaseConfiguration = config;
			DependencyResolver.ConfigurationManager = configurationManager;
#endif

			var azureCommandBusReceiver = new AzureCommandBusReceiver<Guid>(configurationManager, new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), configurationManager, null), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory());
			var handler = new TestCommandSuccessHandler(testResponse);
#if NET472
			azureCommandBusReceiver.RegisterHandler
#else
			await azureCommandBusReceiver.RegisterHandlerAsync
#endif
				<TestCommand>(handler.
#if NET472
					Handle
#else
					HandleAsync
#endif
				, handler.GetType());
			azureCommandBusReceiver.Start();

			var azureCommandBusPublisher = new AzureCommandBusPublisher<Guid>(configurationManager, new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), configurationManager, null), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory());

			// Act
#if NET472
			azureCommandBusPublisher.Publish
#else
			await azureCommandBusPublisher.PublishAsync
#endif
			(command);

			// Assert
			SpinWait.SpinUntil(() => testResponse[processId].Item1);
			Assert.IsNull(testResponse[processId].Item2);
		}

// This test is pointless... it's testing the test itself.
/*
		/// <summary />
		[TestMethod]
		public
#if NET472
			void
#else
			async Task
#endif
			Setup_NoPublishingReceiverOnly_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var command = new TestCommand { Id = processId };
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif
			getTokenConfigurationManager = configurationManager;

			string endpointAddress = configurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.Endpoint");
			string applicationId = configurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.ApplicationId");
			string clientKey = configurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.ClientKey");
			string tentantId = configurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.TenantId");
			string authority = $"https://login.windows.net/{tentantId}";

			string topicName = "test.topic";
			string topicSubscriptionName = "test.tespic.subscription";
			Manager manager = GetManager(tentantId, applicationId, clientKey, endpointAddress, authority);
			IMessageReceiver serviceBusReceiver;
			var eventTopicDescription = new TopicDescription(topicName)
			{
				MaxSizeInMegabytes = 5120,
				DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
				EnablePartitioning = true,
				EnableBatchedOperations = true,
				SupportOrdering = true
			};
#if NET472
			if (!manager.TopicExists(eventTopicDescription.Path))
				manager.CreateTopic(eventTopicDescription);
			if (!manager.SubscriptionExists(eventTopicDescription.Path, topicSubscriptionName))
				manager.CreateSubscription
				(
					new SubscriptionDescription(eventTopicDescription.Path, topicSubscriptionName)
					{
						DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
						EnableBatchedOperations = true,
						EnableDeadLetteringOnFilterEvaluationExceptions = true
					}
				);

#else
			bool topicExists = await manager.TopicExistsAsync(topicName);
			if (!topicExists)
			{
				TopicProperties createdTopic = await manager.CreateTopicAsync(eventTopicDescription);
			}

			bool subscriptionExists = await manager.SubscriptionExistsAsync(topicName, topicSubscriptionName);
			if (!subscriptionExists)
			{
				var subscriptionDescription = new CreateSubscriptionOptions(topicName, topicSubscriptionName)
				{
					DefaultMessageTimeToLive = eventTopicDescription.DefaultMessageTimeToLive,
					EnableBatchedOperations = eventTopicDescription.EnableBatchedOperations,
					DeadLetteringOnMessageExpiration = true,
					LockDuration = new TimeSpan(0, 5, 0)
				};
				SubscriptionProperties createdSubscription = await manager.CreateSubscriptionAsync(subscriptionDescription);
			}
#endif

#if NET472
			// https://github.com/Azure/azure-service-bus/blob/master/samples/DotNet/Microsoft.ServiceBus.Messaging/RoleBasedAccessControl/Program.cs
			serviceBusReceiver = SubscriptionClient.CreateWithAzureActiveDirectory(new Uri(endpointAddress), topicName, topicSubscriptionName, GetToken, authority);
			serviceBusReceiver.OnMessage(message =>
			{
				message.Complete();
			});
			serviceBusReceiver.Close();
#else
			serviceBusReceiver = new MessageReceiver(endpointAddress, EntityNameHelper.FormatSubscriptionPath(topicName, topicSubscriptionName), TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetToken, authority));
			var options = new MessageHandlerOptions((args) => { return Task.FromResult<object>(null); });
			serviceBusReceiver
				.RegisterMessageHandler
				(
					async (message, cancellationToken) =>
					{
						await serviceBusReceiver.CompleteAsync(message.SystemProperties.LockToken);
					},
					options
				);
			serviceBusReceiver.CloseAsync().Wait();
#endif
		}
*/

// This test is pointless... it's testing the test itself.
/*
		/// <summary />
		[TestMethod]
		public
#if NET472
			void
#else
			async Task
#endif
			Setup_PublishOnly_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var command = new TestCommand { Id = processId };
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif
			getTokenConfigurationManager = configurationManager;

			string endpointAddress = configurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.Endpoint");
			string applicationId = configurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.ApplicationId");
			string clientKey = configurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.ClientKey");
			string tentantId = configurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.TenantId");
			string authority = $"https://login.windows.net/{tentantId}";

			string topicName = "test.topic";
			string topicSubscriptionName = "test.tespic.subscription";
			Manager manager;
			TopicClient serviceBusPublish;
			var eventTopicDescription = new TopicDescription(topicName)
			{
#if NET472
				MaxSizeInMegabytes = 5120,
#else
				MaxSizeInMB = 5120,
#endif
				DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
				EnablePartitioning = true,
				EnableBatchedOperations = true,
			};
#if NET472
			manager = new Manager(endpointAddress, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetToken, new Uri(endpointAddress), authority));

			if (!manager.TopicExists(eventTopicDescription.Path))
				manager.CreateTopic(eventTopicDescription);
			if (!manager.SubscriptionExists(eventTopicDescription.Path, topicSubscriptionName))
				manager.CreateSubscription
				(
					new SubscriptionDescription(eventTopicDescription.Path, topicSubscriptionName)
					{
						DefaultMessageTimeToLive = new TimeSpan(0, 25, 0),
						EnableBatchedOperations = true,
						EnableDeadLetteringOnFilterEvaluationExceptions = true
					}
				);

#else
			manager = new Manager(endpointAddress, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetToken, authority));

			Task<bool> checkTask = manager.TopicExistsAsync(topicName);
			checkTask.Wait(1500);
			if (!checkTask.Result)
			{
				Task<TopicDescription> createTopicTask = manager.CreateTopicAsync(eventTopicDescription);
				createTopicTask.Wait(1500);
			}

			checkTask = manager.SubscriptionExistsAsync(topicName, topicSubscriptionName);
			checkTask.Wait(1500);
			if (!checkTask.Result)
			{
				var subscriptionDescription = new SubscriptionDescription(topicName, topicSubscriptionName)
				{
					DefaultMessageTimeToLive = eventTopicDescription.DefaultMessageTimeToLive,
					EnableBatchedOperations = eventTopicDescription.EnableBatchedOperations,
				};
				Task<SubscriptionDescription> createTask = manager.CreateSubscriptionAsync(subscriptionDescription);
				createTask.Wait(1500);
			}
#endif

#if NET472
			// https://github.com/Azure/azure-service-bus/blob/master/samples/DotNet/Microsoft.ServiceBus.Messaging/RoleBasedAccessControl/Program.cs
			serviceBusPublish = TopicClient.CreateWithAzureActiveDirectory(new Uri(endpointAddress), topicName, GetToken, authority);
			serviceBusPublish.Send(new BrokeredMessage("Test Message"));
			serviceBusPublish.Close();
#else
			serviceBusPublish = new TopicClient(endpointAddress, topicName, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetToken, authority));
			serviceBusPublish.SendAsync(new BrokeredMessage(Encoding.UTF8.GetBytes("Test Message"))).Wait();
			serviceBusPublish.CloseAsync().Wait();
#endif
		}
*/

#if NET472
		static IConfigurationManager getTokenConfigurationManager;
		AzureActiveDirectoryTokenProvider.AuthenticationCallback GetToken = async (audience, authority, state) =>
		{
			string applicationId = getTokenConfigurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.ApplicationId");
			string clientKey = getTokenConfigurationManager.GetSetting("Cqrs.Azure.CommandBus.Connection.ClientKey");

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

		/// <summary>
		/// Creates a new instance of <see cref="Manager"/>
		/// </summary>
		protected virtual Manager GetManager(string tenantId, string applicationId, string clientKey, string endpoint, string authority)
		{
			Manager manager;
#if NETSTANDARD2_0 || NET48_OR_GREATER || NET6_0
			var credentials = new ClientSecretCredential(tenantId, applicationId, clientKey);
			manager = new Manager(endpoint, credentials);
#else
			getTokenConfigurationManager = new CloudConfigurationManager();
			manager = new Manager(endpoint, TokenProvider.CreateAzureActiveDirectoryTokenProvider(GetToken, new Uri(endpoint), authority));
#endif
			return manager;
		}
	}
}