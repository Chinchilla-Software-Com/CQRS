using System;
using Cqrs.Events;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	/// <summary>
	/// A series of tests on the <see cref="EventSerialiser{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class EventSerialiserTests
	{
		private const string GoodEventData = "{\"$id\":\"1\",\"$type\":\"Cqrs.Azure.ServiceBus.Tests.Unit.TestEvent, Cqrs.Azure.ServiceBus.Tests.Unit\"}";

		[TestMethod]
		public void SerialisEvent_TestEvent_ExpectedSerialisedData()
		{
			// Arrange
			var eventSerialiser = new EventSerialiser<Guid>();

			// Act
			string data = eventSerialiser.SerialisEvent(new TestEvent());

			// Assert
			Assert.AreEqual(GoodEventData, data);
		}

		[TestMethod]
		public void DeserialisEvent_TestEventData_ExpectedEvent()
		{
			// Arrange
			var eventSerialiser = new EventSerialiser<Guid>();

			// Act
			IEvent<Guid> @event = eventSerialiser.DeserialisEvent(GoodEventData);

			// Assert
			Assert.AreEqual("Cqrs.Azure.ServiceBus.Tests.Unit.TestEvent", @event.GetType().FullName);
		}
	}
}
