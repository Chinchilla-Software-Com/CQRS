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
using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Azure.BlobStorage.DataStores;
using Cqrs.Azure.BlobStorage.Repositories;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Entities;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.Azure.BlobStorage.Test.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="TableStorageDataStore{TData}"/> class
	/// </summary>
	[TestClass]
	public class TableStorageDataStoreTests
	{
		/// <summary>
		/// Tests the <see cref="IDataStore{TData}.Add(TData)"/> method
		/// Passing a valid test <see cref="IEntity"/>
		/// Expecting the test <see cref="IEntity"/> is able to be read.
		/// </summary>
		[TestMethod]
		public virtual void Add_ValidProjectionView_ProjectionViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper(new ContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			TableStorageDataStore<TestEvent> dataStore = CreateDataStore<TestEvent>(logger, new Configuration.ConfigurationManager());

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
			var repository = new TableStorageRepository<TestQueryStrategy, TestQueryBuilder<TestEvent>, TestEvent>(() => dataStore, null);
			timer.Start();
			TestEvent view = repository.Load(event1.Rsn);
			timer.Stop();
			Console.WriteLine("Load operation took {0}", timer.Elapsed);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Id, view.Id);
		}

		/// <summary>
		/// Tests the <see cref="IDataStore{TData}.Add(TData)"/> method
		/// Passing a valid test <see cref="IEntity"/>
		/// Expecting the test <see cref="IEntity"/> is able to be read.
		/// </summary>
		[TestMethod]
		public virtual void Add_ValidProjectionEntityView_ProjectionEntityViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper(new ContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			TableStorageDataStore<TestEntity> dataStore = CreateDataStore<TestEntity>(logger, new Configuration.ConfigurationManager());

			var event1 = new TestEntity
			{
				Rsn = Guid.NewGuid(),
				Name = "Name"
			};

			// Act
			dataStore.Add(event1);

			// Assert
			var timer = new Stopwatch();
			var repository = new TableStorageRepository<TestQueryStrategy, TestQueryBuilder<TestEntity>, TestEntity>(() => dataStore, null);
			timer.Start();
			TestEntity view = repository.Load(event1.Rsn);
			timer.Stop();
			Console.WriteLine("Load operation took {0}", timer.Elapsed);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Rsn, view.Rsn);
			Assert.AreEqual(event1.Name, view.Name);
		}

		/// <summary>
		/// Tests the <see cref="IDataStore{TData}.Update(TData)"/> method
		/// Passing a valid test <see cref="IEntity"/>
		/// Expecting the test <see cref="IEntity"/> is able to be read with updated properties.
		/// </summary>
		[TestMethod]
		public virtual void Update_ValidProjectionEntityView_ProjectionEntityViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper(new ContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			TableStorageDataStore<TestEntity> dataStore = CreateDataStore<TestEntity>(logger, new Configuration.ConfigurationManager());

			var event1 = new TestEntity
			{
				Rsn = Guid.NewGuid(),
				Name = "Name1"
			};
			dataStore.Add(event1);

			// The repo disposes the datastore, so a copy is needed.
			TableStorageDataStore<TestEntity> repoDataStore = CreateDataStore<TestEntity>(logger, new Configuration.ConfigurationManager());
			// DO NOT REMOVE/REFACTOR Closure modifier access thingee stuff...
			var store = repoDataStore;
			var repository = new TableStorageRepository<TestQueryStrategy, TestQueryBuilder<TestEntity>, TestEntity>(() => store, null);
			TestEntity view = repository.Load(event1.Rsn);
			view.Name = "Name2";

			// Act
			dataStore.Update(event1);

			// Assert
			var timer = new Stopwatch();
			timer.Start();
			// Refresh the data store due to disposal.
			repoDataStore = CreateDataStore<TestEntity>(logger, new Configuration.ConfigurationManager());
			repository = new TableStorageRepository<TestQueryStrategy, TestQueryBuilder<TestEntity>, TestEntity>(() => repoDataStore, null);
			view = repository.Load(event1.Rsn);
			timer.Stop();
			Console.WriteLine("Load operation took {0}", timer.Elapsed);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Rsn, view.Rsn);
			Assert.AreEqual(event1.Name, view.Name);
		}

		/// <summary>
		/// Create a <see cref="TableStorageDataStore{TData}"/> ready for testing.
		/// </summary>
		protected virtual TableStorageDataStore<TData> CreateDataStore<TData>(ILogger logger, IConfigurationManager configurationManager)
			where TData : Entity
		{
			return new TableStorageDataStore<TData>(logger, new TableStorageDataStoreConnectionStringFactory(configurationManager, logger));
		}
	}
}