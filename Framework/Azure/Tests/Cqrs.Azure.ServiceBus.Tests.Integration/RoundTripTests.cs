using System;
using System.Collections.Generic;
using System.Threading;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using cdmdotnet.StateManagement.Threaded;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Events;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

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

			var azureEventBusReceiver = new AzureEventBusReceiver<Guid>(new ConfigurationManager(), new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(new ConfigurationManager(), new ThreadedContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), new ConfigurationManager(), null), new BusHelper(new ConfigurationManager(), new ThreadedContextItemCollectionFactory()));
			var handler = new TestEventSuccessHandler(testResponse);
			azureEventBusReceiver.RegisterHandler<TestEvent>(handler.Handle, handler.GetType());
			azureEventBusReceiver.Start();

			var azureEventBusPublisher = new AzureEventBusPublisher<Guid>(new ConfigurationManager(), new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(new ConfigurationManager(), new ThreadedContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), new ConfigurationManager(), null), new BusHelper(new ConfigurationManager(), new ThreadedContextItemCollectionFactory()));

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

			var azureCommandBusReceiver = new AzureCommandBusReceiver<Guid>(new ConfigurationManager(), new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(new ConfigurationManager(), new ThreadedContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), new ConfigurationManager(), null), new BusHelper(new ConfigurationManager(), new ThreadedContextItemCollectionFactory()));
			var handler = new TestCommandSuccessHandler(testResponse);
			azureCommandBusReceiver.RegisterHandler<TestCommand>(handler.Handle, handler.GetType());
			azureCommandBusReceiver.Start();

			var azureCommandBusPublisher = new AzureCommandBusPublisher<Guid>(new ConfigurationManager(), new MessageSerialiser<Guid>(), new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new AzureBusHelper<Guid>(new GuidAuthenticationTokenHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()), new MessageSerialiser<Guid>(), new BusHelper(new ConfigurationManager(), new ThreadedContextItemCollectionFactory()), new BuiltInHashAlgorithmFactory(), new ConfigurationManager(), null), new BusHelper(new ConfigurationManager(), new ThreadedContextItemCollectionFactory()));

			// Act
			azureCommandBusPublisher.Publish(command);

			// Assert
			SpinWait.SpinUntil(() => testResponse[processId].Item1);
			Assert.IsNull(testResponse[processId].Item2);
		}
	}
}