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
	/// A series of tests on the <see cref="BlobStorageDataStore{TData}"/> class
	/// </summary>
	[TestClass]
	public class BlobStorageDataStoreTests
	{
		/// <summary>
		/// Tests the <see cref="IDataStore{TData}.Add(TData)"/> method
		/// Passing a valid test <see cref="IEntity"/>
		/// Expecting the test <see cref="IEntity"/> is able to be read.
		/// </summary>
		[TestMethod]
		public virtual void Save_ValidProjectionView_ProjectionViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper(new ContextItemCollectionFactory());
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			var dataStore = new BlobStorageDataStore<TestEvent>(logger, new BlobStorageDataStoreConnectionStringFactory(new Configuration.ConfigurationManager(), logger));

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
			TestEvent view = dataStore.GetByName(event1.Rsn);
			timer.Stop();
			Console.WriteLine("Load operation took {0}", timer.Elapsed);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Id, view.Id);
		}
	}
}