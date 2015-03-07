using System;
using System.Collections.Generic;
using System.Threading;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Configuration;
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
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			// Arrange
			IDictionary<Guid, Tuple<bool, Exception>> testResponse = new Dictionary<Guid, Tuple<bool, Exception>>();

			Guid processId = Guid.NewGuid();
			testResponse.Add(processId, new Tuple<bool, Exception>(false, null));
			var @event = new TestEvent{Id = processId};

			var azureMessageBusReceiver = new AzureMessageBusReceiver<Guid>(new ConfigurationManager(), new EventSerialiser<Guid>());
			azureMessageBusReceiver.RegisterHandler<TestEvent>(new TestEventSuccessHandler(testResponse).Handle);

			var azureMessageBusPublisher = new AzureMessageBusPublisher<Guid>(new ConfigurationManager(), new EventSerialiser<Guid>());

			// Act
			azureMessageBusPublisher.Publish(@event);

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
}
