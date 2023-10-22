#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Azure.Storage.Events;
using Cqrs.Configuration;
using Cqrs.Events;
using NUnit.Framework;

using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

#if NET472_OR_GREATER
#else
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Azure.Storage.Test.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="BlobStorageEventStore{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class BlobStorageEventStoreTests
	{
		/// <summary>
		/// Tests the <see cref="IEventStore{TAuthenticationToken}.Save"/> method
		/// Passing a valid test <see cref="IEvent{TAuthenticationToken}"/>
		/// Expecting the test <see cref="IEvent{TAuthenticationToken}"/> is able to be read.
		/// </summary>
		[TestMethod]
		public virtual
#if NET472
			void
#else
			async Task
#endif
				Save_ValidEvent_EventCanBeRetreived()
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
			DependencyResolver.ConfigurationManager = configurationManager;
#endif

			var correlationIdHelper = new CorrelationIdHelper(new ContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			var eventStore = new BlobStorageEventStore<Guid>(new DefaultEventBuilder<Guid>(), new EventDeserialiser<Guid>(), logger, new BlobStorageEventStoreConnectionStringFactory(configurationManager, logger));

			var event1 = new TestEvent
			{
				Rsn = Guid.NewGuid(),
				Id = Guid.NewGuid(),
				CorrelationId = correlationIdHelper.GetCorrelationId(),
				Frameworks = new List<string> { "Test 1" },
				TimeStamp = DateTimeOffset.UtcNow
			};
			var event2 = new TestEvent
			{
				Rsn = Guid.NewGuid(),
				Id = Guid.NewGuid(),
				CorrelationId = correlationIdHelper.GetCorrelationId(),
				Frameworks = new List<string> { "Test 2" },
				TimeStamp = DateTimeOffset.UtcNow
			};

			// Act
#if NET472
			eventStore.Save<TestEvent>(event1);
			eventStore.Save<TestEvent>(event2);
#else
			await eventStore.SaveAsync<TestEvent>(event1);
			await eventStore.SaveAsync<TestEvent>(event2);
#endif

			// Assert
			var timer = new Stopwatch();
			IList<IEvent<Guid>> events =
			(
#if NET472
				eventStore.Get
#else
				await eventStore.GetAsync
#endif
					<TestEvent>(event1.Id)
			).ToList();
			timer.Stop();
			Console.WriteLine("Load one operation took {0}", timer.Elapsed);
			Assert.AreEqual(1, events.Count);
			Assert.AreEqual(event1.Id, events.Single().Id);
			Assert.AreEqual(event1.Frameworks.Single(), events.Single().Frameworks.Single());

			timer.Restart();
			events =
			(
#if NET472
				eventStore.Get
#else
				await eventStore.GetAsync
#endif
					<TestEvent>(event2.Id)
			).ToList();
			timer.Stop();
			Console.WriteLine("Load one operation took {0}", timer.Elapsed);
			Assert.AreEqual(1, events.Count);
			Assert.AreEqual(event2.Id, events.Single().Id);
			Assert.AreEqual(event2.Frameworks.Single(), events.Single().Frameworks.Single());

			timer.Restart();
			IList<EventData> correlatedEvents =
			(
#if NET472
				eventStore.Get
#else
				await eventStore.GetAsync
#endif
					(event1.CorrelationId)
			).ToList();
			timer.Stop();
			Console.WriteLine("Load several correlated operation took {0}", timer.Elapsed);
			Assert.AreEqual(2, correlatedEvents.Count);
		}
	}
}