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
using Cqrs.DataStores;
using Cqrs.Entities;
using Cqrs.MongoDB.DataStores;
using MongoDB.Driver;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.MongoDB.Tests.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="MongoDbDataStore{TData}"/> class
	/// </summary>
	[TestClass]
	public class MongoDbDataStoreTests
	{
		/// <summary>
		/// Tests the <see cref="IDataStore{TData}.Add(TData)"/> method
		/// Passing a valid test <see cref="IEntity"/>
		/// Expecting the test <see cref="IEntity"/> is able to be read.
		/// </summary>
		[TestMethod]
		public void Add_ValidProjectionView_ProjectionViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper(new ContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettings(), correlationIdHelper);

			var connectionStringFactory = new TestMongoDataStoreConnectionStringFactory();
			TestMongoDataStoreConnectionStringFactory.DatabaseName = string.Format("Test-{0}", new Random().Next(0, 9999));

			var factory = new TestMongoDbDataStoreFactory(logger, connectionStringFactory);
			IMongoCollection<TestEvent> collection = factory.GetTestEventCollection();
			try
			{
				// Arrange
				var dataStore = new MongoDbDataStore<TestEvent>(logger, collection);

				var event1 = new TestEvent
				{
					Rsn = Guid.NewGuid(),
					Id = Guid.NewGuid(),
					CorrelationId = correlationIdHelper.GetCorrelationId(),
					Frameworks = new List<string> { "Test 1" },
					TimeStamp = DateTimeOffset.UtcNow
				};

				// Act
				dataStore.Add(event1);

				// Assert
				var timer = new Stopwatch();
				timer.Start();
				TestEvent view = dataStore.SingleOrDefault(e => e.Rsn == event1.Rsn);
				timer.Stop();
				Console.WriteLine("Load operation took {0}", timer.Elapsed);
				Assert.IsNotNull(view);
				Assert.AreEqual(event1.Id, view.Id);
			}
			finally
			{
				// Clean-up
				collection.Database.Client.DropDatabase(TestMongoDataStoreConnectionStringFactory.DatabaseName);
			}
		}
	}
}