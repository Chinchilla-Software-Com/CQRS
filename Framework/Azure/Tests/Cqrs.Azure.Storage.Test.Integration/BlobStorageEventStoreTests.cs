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

using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement.Threaded;
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
using NUnit.Framework.Internal.Execution;


#if NET472
#else
using System.Threading.Tasks;
#endif

#if NET472_OR_GREATER
#else
using Cqrs.Azure.ConfigurationManager;
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

			IList<TestEvent> eventsToSend = new List<TestEvent>();
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			for (int i = 0; i < 40; i++)
			{
				var event1 = new TestEvent
				{
					Rsn = i % 2 == 1 ? id1 : id2,
					Id = i % 2 == 1 ? id1 : id2,
					CorrelationId = correlationIdHelper.GetCorrelationId(),
					Frameworks = new List<string> { $"Test {i}" },
					TimeStamp = DateTimeOffset.UtcNow
				};
				eventsToSend.Add(event1);
			}

			// Act
			foreach(var @event in eventsToSend)
			{
#if NET472
				eventStore.Save<TestEvent>(@event);
#else
				await eventStore.SaveAsync<TestEvent>(@event);
#endif
			}

			// Assert
			var timer = new Stopwatch();
			IList<IEvent<Guid>> events =
			(
#if NET472
				eventStore.Get
#else
				await eventStore.GetAsync
#endif
					<TestEvent>(id1)
			).ToList();
			timer.Stop();
			Console.WriteLine("Load one operation took {0}", timer.Elapsed);
			Assert.AreEqual(20, events.Count);
			Assert.IsTrue(events.All(x => x.Id == id1));

			timer.Restart();
			events =
			(
#if NET472
				eventStore.Get
#else
				await eventStore.GetAsync
#endif
					<TestEvent>(id2)
			).ToList();
			timer.Stop();
			Console.WriteLine("Load one operation took {0}", timer.Elapsed);
			Assert.AreEqual(20, events.Count);
			Assert.IsTrue(events.All(x => x.Id == id2));

			timer.Restart();
			IList<EventData> correlatedEvents =
			(
#if NET472
				eventStore.Get
#else
				await eventStore.GetAsync
#endif
					(correlationIdHelper.GetCorrelationId())
			).ToList();
			timer.Stop();
			Console.WriteLine("Load several correlated operation took {0}", timer.Elapsed);
			Assert.AreEqual(40, correlatedEvents.Count);
		}
	}
}