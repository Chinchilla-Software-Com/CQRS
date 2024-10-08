#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

using Chinchilla.Logging;
using Chinchilla.Logging.Azure.Configuration;
using Chinchilla.Logging.Configuration;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Configuration;

using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

#if NET472
using Manager = Microsoft.ServiceBus.NamespaceManager;
#else
using Cqrs.Azure.ConfigurationManager;
using Microsoft.Extensions.Configuration;
using Manager = Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient;
using System.Threading.Tasks;
#endif

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	/// <summary>
	/// A series of tests on the <see cref="MessageSerialiser{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class AzureServiceBusTests
	{
		/// <summary>
		/// </summary>
		[TestMethod]
		public void Constructor_NothingSpecial_SafeContainerName()
		{
			// Arrange
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