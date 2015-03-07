using System;
using Cqrs.Commands;
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
	/// A series of tests on the <see cref="MessageSerialiser{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class MessageSerialiserTests
	{
		private const string GoodEventData = "{\"$id\":\"1\",\"$type\":\"Cqrs.Azure.ServiceBus.Tests.Unit.TestEvent, Cqrs.Azure.ServiceBus.Tests.Unit\",\"Id\":\"a3008122-b365-45ef-86ec-865df098b886\"}";

		private const string GoodCommandData = "{\"$id\":\"1\",\"$type\":\"Cqrs.Azure.ServiceBus.Tests.Unit.TestCommand, Cqrs.Azure.ServiceBus.Tests.Unit\",\"Id\":\"17c0585b-3b24-4865-9afc-5fa97e36606a\"}";

		[TestMethod]
		public void SerialisEvent_TestEvent_ExpectedSerialisedData()
		{
			// Arrange
			var eventSerialiser = new MessageSerialiser<Guid>();

			// Act
			string data = eventSerialiser.SerialiseEvent(new TestEvent { Id = new Guid("a3008122-b365-45ef-86ec-865df098b886") });

			// Assert
			Assert.AreEqual(GoodEventData, data);
		}

		[TestMethod]
		public void DeserialisEvent_TestEventData_ExpectedEvent()
		{
			// Arrange
			var eventSerialiser = new MessageSerialiser<Guid>();

			// Act
			IEvent<Guid> @event = eventSerialiser.DeserialiseEvent(GoodEventData);

			// Assert
			Assert.AreEqual("Cqrs.Azure.ServiceBus.Tests.Unit.TestEvent", @event.GetType().FullName);
		}

		[TestMethod]
		public void SerialisCommand_TestCommand_ExpectedSerialisedData()
		{
			// Arrange
			var command = new MessageSerialiser<Guid>();

			// Act
			string data = command.SerialiseCommand(new TestCommand { Id = new Guid("17c0585b-3b24-4865-9afc-5fa97e36606a") });

			// Assert
			Assert.AreEqual(GoodCommandData, data);
		}

		[TestMethod]
		public void DeserialisCommand_TestCommandData_ExpectedCommand()
		{
			// Arrange
			var commandSerialiser = new MessageSerialiser<Guid>();

			// Act
			ICommand<Guid> command = commandSerialiser.DeserialiseCommand(GoodCommandData);

			// Assert
			Assert.AreEqual("Cqrs.Azure.ServiceBus.Tests.Unit.TestCommand", command.GetType().FullName);
		}
	}
}
