#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Text.RegularExpressions;

using Cqrs.Azure.ConfigurationManager;
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

namespace Cqrs.Azure.ServiceBus.Tests.Unit
{
	/// <summary>
	/// A series of tests on the <see cref="MessageSerialiser{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class MessageSerialiserTests
	{
		private const string GoodEventData = "{\"$id\":\"1\",\"$type\":\"Cqrs.Azure.ServiceBus.Tests.Unit.TestEvent, Cqrs.Azure.ServiceBus.Tests.Unit\",\"Id\":\"a3008122-b365-45ef-86ec-865df098b886\",\"SensitiveDecimal\":\"Ed1c5OdvgV1+JXF9gQB90y3cju0nra+d2beaH1lcMII=\"}";
		private const string GoodEventDataRegExPattern = "{\"\\$id\":\"1\",\"\\$type\":\"Cqrs\\.Azure\\.ServiceBus\\.Tests\\.Unit\\.TestEvent, Cqrs\\.Azure\\.ServiceBus\\.Tests\\.Unit\",\"Id\":\"a3008122-b365-45ef-86ec-865df098b886\",\"SensitiveDecimal\":\"(\\w|\\+|=|-)+\"}";

		private const string GoodCommandData = "{\"$id\":\"1\",\"$type\":\"Cqrs.Azure.ServiceBus.Tests.Unit.TestCommand, Cqrs.Azure.ServiceBus.Tests.Unit\",\"Id\":\"17c0585b-3b24-4865-9afc-5fa97e36606a\"}";

		/// <summary>
		/// Tests the <see cref="MessageSerialiser{TAuthenticationToken}.SerialiseEvent{TEvent}"/> method
		/// Passing a valid test <see cref="IEvent{TAuthenticationToken}"/>
		/// Expecting the serialised data is as expected.
		/// </summary>
		[TestMethod]
		public void SerialiseEvent_TestEventWithEncryption_ExpectedSerialisedData()
		{
			// Arrange
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif
			DependencyResolver.ConfigurationManager = configurationManager;
			var eventSerialiser = new MessageSerialiser<Guid>();

			// Act
			string data = eventSerialiser.SerialiseEvent(new TestEvent { Id = new Guid("a3008122-b365-45ef-86ec-865df098b886"), SensitiveDecimal = 62.35M });

			// Assert
			Assert.IsTrue(new Regex(GoodEventDataRegExPattern).IsMatch(data));
		}

		/// <summary>
		/// Tests the <see cref="MessageSerialiser{TAuthenticationToken}.DeserialiseEvent"/> method
		/// Passing a valid test string of JSON
		/// Expecting the data deserialised to the expected <see cref="Type"/>.
		/// </summary>
		[TestMethod]
		public void DeserialiseEvent_TestEventDataWithEncryptedValues_ExpectedEvent()
		{
			// Arrange
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif
			DependencyResolver.ConfigurationManager = configurationManager;
			var eventSerialiser = new MessageSerialiser<Guid>();

			// Act
			IEvent<Guid> @event = eventSerialiser.DeserialiseEvent(GoodEventData);

			// Assert
			Assert.AreEqual("Cqrs.Azure.ServiceBus.Tests.Unit.TestEvent", @event.GetType().FullName);
			Assert.AreEqual(new Guid("a3008122-b365-45ef-86ec-865df098b886"), @event.Id);
			Assert.AreEqual(62.35M, ((TestEvent)@event).SensitiveDecimal);
		}

		/// <summary>
		/// Tests the <see cref="MessageSerialiser{TAuthenticationToken}.SerialiseCommand{TEvent}"/> method
		/// Passing a valid test <see cref="ICommand{TAuthenticationToken}"/>
		/// Expecting the serialised data is as expected.
		/// </summary>
		[TestMethod]
		public void SerialiseCommand_TestCommand_ExpectedSerialisedData()
		{
			// Arrange
			var commandSerialiser = new MessageSerialiser<Guid>();

			// Act
			string data = commandSerialiser.SerialiseCommand(new TestCommand { Id = new Guid("17c0585b-3b24-4865-9afc-5fa97e36606a") });

			// Assert
			Assert.AreEqual(GoodCommandData, data);
		}

		/// <summary>
		/// Tests the <see cref="MessageSerialiser{TAuthenticationToken}.DeserialiseCommand"/> method
		/// Passing a valid test string of JSON
		/// Expecting the data deserialised to the expected <see cref="Type"/>.
		/// </summary>
		[TestMethod]
		public void DeserialiseCommand_TestCommandData_ExpectedCommand()
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