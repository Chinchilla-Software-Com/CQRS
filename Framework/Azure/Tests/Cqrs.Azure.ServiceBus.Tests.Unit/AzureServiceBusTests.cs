#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Chinchilla.Logging;
using Chinchilla.Logging.Azure.Configuration;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;

using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
using NUnit.Framework;

#if NET472
using Microsoft.ServiceBus.Messaging;
using Manager = Microsoft.ServiceBus.NamespaceManager;
#else
using Cqrs.Azure.ConfigurationManager;
using Microsoft.Extensions.Configuration;
using Manager = Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient;
using BrokeredMessage = Azure.Messaging.ServiceBus.ServiceBusMessage;
#endif

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	/// <summary>
	/// A series of tests on the <see cref="MessageSerialiser{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class AzureServiceBusTests
	{
		/// <summary />
		[TestMethod]
		public async Task Constructor_NothingSpecial_SafeContainerName()
		{
			// Arrange
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.json", optional: true, reloadOnChange: false)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif
			DependencyResolver.ConfigurationManager = configurationManager;
			var logger = new MockLogger(new AzureLoggerSettingsConfiguration(
#if NET472
#else
				config
#endif
				) , new NullCorrelationIdHelper(), new NullTelemetryHelper());

			// Act
			var azureServiceBus = new MockAzureServiceBus(configurationManager, null, null, new NullCorrelationIdHelper(), logger, null, null, null, true);

			// Assert
			const string expectedValue =
#if NET472
			"Cqrs.Azure.ServiceBus.AzureServiceBus`1.CheckPrivateTopicExists";
#else
			"Cqrs.Azure.ServiceBus.AzureServiceBus`1.CheckPrivateTopicExistsAsync";
#endif

			Assert.IsTrue(logger.FoundContainers.Contains(expectedValue));

			await Task.CompletedTask;
		}

		/// <summary />
		[TestMethod]
		public async Task CreateBrokeredMessageAsync_SagaEvent_HelpfulTypeCalculated()
		{
			// Arrange
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.json", optional: true, reloadOnChange: false)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif
			DependencyResolver.ConfigurationManager = configurationManager;
			var logger = new MockLogger(new AzureLoggerSettingsConfiguration(
#if NET472
#else
				config
#endif
				), new NullCorrelationIdHelper(), new NullTelemetryHelper());

			var azureServiceBus = new MockAzureServiceBus(configurationManager, new MessageSerialiser<Guid>(), new DefaultAuthenticationTokenHelper(new ContextItemCollectionFactory()), new NullCorrelationIdHelper(), logger, null, null, new BuiltInHashAlgorithmFactory(), true);

			// Act
			var result =
#if NET472
				azureServiceBus._CreateBrokeredMessage
#else
				await azureServiceBus._CreateBrokeredMessageAsync
#endif
					(message => { return string.Empty; }, typeof(SagaEvent<Guid>), new SagaEvent<Guid>(new TestEvent()));

			//Assert

			await Task.CompletedTask;
		}
	}

	class MockAzureServiceBus : AzureServiceBus<Guid>
	{
		public MockAzureServiceBus(IConfigurationManager configurationManager, IMessageSerialiser<Guid> messageSerialiser, IAuthenticationTokenHelper<Guid> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<Guid> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory, isAPublisher)
		{
		}

		protected override string MessageBusConnectionStringConfigurationKey => "Cqrs.Noise";

		protected override string MessageBusConnectionEndpointConfigurationKey => "Cqrs.Noise";

		protected override string MessageBusConnectionApplicationIdConfigurationKey => "Cqrs.Noise";

		protected override string MessageBusConnectionClientKeyConfigurationKey => "Cqrs.Noise";

		protected override string MessageBusConnectionTenantIdConfigurationKey => "Cqrs.Noise";

		protected override string SigningTokenConfigurationKey => "Cqrs.Noise";

		protected override string PrivateTopicNameConfigurationKey => "Cqrs.Noise";

		protected override string PublicTopicNameConfigurationKey => "Cqrs.Noise";

		protected override string DefaultPrivateTopicName => "Cqrs.Noise";

		protected override string DefaultPublicTopicName => "Cqrs.Noise";

		protected override string PrivateTopicSubscriptionNameConfigurationKey => "Cqrs.PrivateTopicSubscriptionName";

		protected override string PublicTopicSubscriptionNameConfigurationKey => "Cqrs.PublicTopicSubscriptionName";

		protected override string ThrowExceptionOnReceiverMessageLockLostExceptionDuringCompleteConfigurationKey => "Cqrs.Noise";

		protected override
#if NET472
			void CheckTopicExists
#else
			async Task CheckTopicExistsAsync
#endif
			(Manager manager, string topicName, string subscriptionName, bool createSubscriptionIfNotExists = true)
		{
#if NET472
#else
			await Task.CompletedTask;
#endif
		}

		protected override
#if NET472
			void InstantiatePublishing
#else
			async Task InstantiatePublishingAsync
#endif
	()
		{
#if NET472
			CheckPrivateTopicExists(null, false);
			CheckPublicTopicExists(null, false);
#else
			await CheckPrivateTopicExistsAsync(null, false);
			await CheckPublicTopicExistsAsync(null, false);
#endif
		}

		public virtual
#if NET472
			BrokeredMessage _CreateBrokeredMessage
#else
			async Task<BrokeredMessage> _CreateBrokeredMessageAsync
#endif
			<TMessage>(Func<TMessage, string> serialiserFunction, Type messageType, TMessage message
#if NET472
#else
				, TimeSpan? delay = null
#endif
			)
		{
			return
#if NET472
				base.CreateBrokeredMessage
#else
				await base.CreateBrokeredMessageAsync
#endif
					(serialiserFunction, messageType, message
#if NET472
#else
				, delay
#endif
			);
		}
	}
}

namespace Chinchilla.Logging
{
	class MockLogger : ConsoleLogger
	{
		public MockLogger(ILoggerSettings loggerSettings, ICorrelationIdHelper correlationIdHelper, ITelemetryHelper telemetryHelper = null)
			: base(loggerSettings, correlationIdHelper, telemetryHelper)
		{
			FoundContainers = new List<string>();
		}

		public IList<string> FoundContainers { get; }

		protected override string UseOrBuildContainerName(string container)
		{
			string containerName = base.UseOrBuildContainerName(container);
			FoundContainers.Add(containerName);
			return containerName;
		}
	}
}