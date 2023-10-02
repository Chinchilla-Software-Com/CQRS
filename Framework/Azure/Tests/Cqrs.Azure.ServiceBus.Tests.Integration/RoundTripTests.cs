using System;
using System.Collections.Generic;
using System.Text;
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
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.Identity.Client;
using NUnit.Framework;

#if NET472
using Manager = Microsoft.ServiceBus.NamespaceManager;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
#else
using Manager = Microsoft.Azure.ServiceBus.Management.ManagementClient;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Primitives;
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
		public void Publish_TestEvent_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var @event = new TestEvent{Id = processId};
			IConfigurationManager configurationManager;
#if NET472
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
			DependencyResolver.ConfigurationManager = configurationManager;
#endif

			var azureEventBusReceiver = new AzureEventBusReceiver<Guid>(configurationManager, new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), configurationManager, null), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory());
			var handler = new TestEventSuccessHandler(testResponse);
			azureEventBusReceiver.RegisterHandler<TestEvent>(handler.Handle, handler.GetType());
			azureEventBusReceiver.Start();

			var azureEventBusPublisher = new AzureEventBusPublisher<Guid>(configurationManager, new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), configurationManager, null), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory());

			// Act
			azureEventBusPublisher.Publish(@event);

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
		public void Publish_TestCommand_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var command = new TestCommand { Id = processId };
			IConfigurationManager configurationManager;
#if NET472
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
			Configuration.ConfigurationManager.Configuration = config;
			DependencyResolver.ConfigurationManager = configurationManager;
#endif

			var azureCommandBusReceiver = new AzureCommandBusReceiver<Guid>(configurationManager, new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), configurationManager, null), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory());
			var handler = new TestCommandSuccessHandler(testResponse);
			azureCommandBusReceiver.RegisterHandler<TestCommand>(handler.Handle, handler.GetType());
			azureCommandBusReceiver.Start();

			var azureCommandBusPublisher = new AzureCommandBusPublisher<Guid>(configurationManager, new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), configurationManager, null), new BusHelper(configurationManager, new ContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory());

			// Act
			azureCommandBusPublisher.Publish(command);

			// Assert
			SpinWait.SpinUntil(() => testResponse[processId].Item1);
			Assert.IsNull(testResponse[processId].Item2);
		}

		/// <summary />
		[TestMethod]
		public void Setup_NoPublishingReceiverOnly_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var command = new TestCommand { Id = processId };
			IConfigurationManager configurationManager;
#if NET472
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
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
			IMessageReceiver serviceBusReceiver;
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

		/// <summary />
		[TestMethod]
		public void Setup_PublishOnly_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var command = new TestCommand { Id = processId };
			IConfigurationManager configurationManager;
#if NET472
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
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
	}
}