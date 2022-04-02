using System;
using System.Collections.Generic;
using System.Threading;
using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#if NET472
#else
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
	}
}