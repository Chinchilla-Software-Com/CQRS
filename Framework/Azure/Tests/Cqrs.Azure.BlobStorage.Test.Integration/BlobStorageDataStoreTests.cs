using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Azure.BlobStorage.DataStores;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Configuration;
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
		[TestMethod]
		public void Save_ValidProjectionView_ProjectionViewCanBeRetreived()
		{
			// Arrange
			var correlationIdHelper = new CorrelationIdHelper();
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			var logger = new ConsoleLogger(new LoggerSettingsConfigurationSection(), correlationIdHelper);
			var dataStore = new BlobStorageDataStore<TestEvent>(logger, new BlobStorageDataStoreConnectionStringFactory(new ConfigurationManager(), logger));

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
			TestEvent view = dataStore.GetByName(event1.Rsn);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Id, view.Id);
		}
	}
}