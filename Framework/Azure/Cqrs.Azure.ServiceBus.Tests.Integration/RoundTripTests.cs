using System;
using System.Collections.Generic;
using System.Threading;
using Cqrs.Authentication;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Messages;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.Azure.ServiceBus.Tests.Integration
{
	[TestClass]
	public class RoundTripTests
	{
		public class GuidSingleSignOnTokenValueHelper : IAuthenticationTokenHelper<Guid>
		{
			#region IAuthenticationTokenHelper<Guid> Members

			public Guid GetAuthenticationToken()
			{
				return Guid.Empty;
			}

			public Guid SetAuthenticationToken(Guid permissionScope)
			{
				return Guid.Empty;
			}

			#endregion
		}

		[TestMethod]
		public void Publish_TestEvent_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var @event = new TestEvent{Id = processId};

			var azureEventBusReceiver = new AzureEventBusReceiver<Guid>(new ConfigurationManager(), new MessageSerialiser<Guid>(), new GuidSingleSignOnTokenValueHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()));
			azureEventBusReceiver.RegisterHandler<TestEvent>(new TestEventSuccessHandler(testResponse).Handle);

			var azureEventBusPublisher = new AzureEventBusPublisher<Guid>(new ConfigurationManager(), new MessageSerialiser<Guid>(), new GuidSingleSignOnTokenValueHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()));

			// Act
			azureEventBusPublisher.Publish(@event);

			// Assert
			SpinWait.SpinUntil(() => testResponse[processId].Item1);
			Assert.IsNull(testResponse[processId].Item2);
		}

		[TestMethod]
		public void Publish_TestCommand_NoExceptions()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var command = new TestCommand { Id = processId };

			var azureCommandBusReceiver = new AzureCommandBusReceiver<Guid>(new ConfigurationManager(), new MessageSerialiser<Guid>(), new GuidSingleSignOnTokenValueHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()));
			azureCommandBusReceiver.RegisterHandler<TestCommand>(new TestCommandSuccessHandler(testResponse).Handle);

			var azureCommandBusPublisher = new AzureCommandBusPublisher<Guid>(new ConfigurationManager(), new MessageSerialiser<Guid>(), new GuidSingleSignOnTokenValueHelper(), new NullCorrelationIdHelper(), new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper()));

			// Act
			azureCommandBusPublisher.Send(command);

			// Assert
			SpinWait.SpinUntil(() => testResponse[processId].Item1);
			Assert.IsNull(testResponse[processId].Item2);
		}
	}

	public class TestEventSuccessHandler : IHandler<TestEvent>
	{
		public TestEventSuccessHandler(IDictionary<Guid, Tuple<bool, Exception>> testResponse)
		{
			TestResponse = testResponse;
		}

		protected IDictionary<Guid, Tuple<bool, Exception>> TestResponse { get; private set; }

		#region Implementation of IHandler<in TestEvent>

		public void Handle(TestEvent message)
		{
			TestResponse[message.Id] = new Tuple<bool, Exception>(true, null);
		}

		#endregion
	}

	public class TestCommandSuccessHandler : IHandler<TestCommand>
	{
		public TestCommandSuccessHandler(IDictionary<Guid, Tuple<bool, Exception>> testResponse)
		{
			TestResponse = testResponse;
		}

		protected IDictionary<Guid, Tuple<bool, Exception>> TestResponse { get; private set; }

		#region Implementation of IHandler<in TestCommand>

		public void Handle(TestCommand message)
		{
			TestResponse[message.Id] = new Tuple<bool, Exception>(true, null);
		}

		#endregion
	}
}
