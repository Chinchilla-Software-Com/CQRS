#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Text.RegularExpressions;

using Cqrs.Azure.KeyVault;
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
using Cqrs.Azure.ConfigurationManager;
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
		private const string GoodEventData = "{\"$id\":\"1\",\"$type\":\"Cqrs.Azure.ServiceBus.Tests.Unit.TestEvent, Cqrs.Azure.ServiceBus.Tests.Unit\",\"Id\":\"a3008122-b365-45ef-86ec-865df098b886\",\"SensitiveDecimal\":\"Tofnx6ngI1bM+zfBC0IVn6/wKsfweyOQ7mTMJxKza28=\"}";
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
#if NET472
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif

			var eventSerialiser = new MessageSerialiser<Guid>();
			JsonSerialisationConfigurator.Configure(configurationManager);

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
#if NET472
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif

			var eventSerialiser = new MessageSerialiser<Guid>();
			JsonSerialisationConfigurator.Configure(configurationManager);

			// Act
			IEvent<Guid> @event = eventSerialiser.DeserialiseEvent(GoodEventData);

			// Assert
			Assert.AreEqual("Cqrs.Azure.ServiceBus.Tests.Unit.TestEvent", @event.GetType().FullName);
			Assert.AreEqual(new Guid("a3008122-b365-45ef-86ec-865df098b886"), @event.Id);
			Assert.AreEqual(62.35M, ((TestEvent)@event).SensitiveDecimal);
		}
	}
}