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
using System.Threading.Tasks;

using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Azure.Storage.DataStores;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Entities;

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
		public virtual
#if NET472
			void
#else
			async Task
#endif
				Add_ValidProjectionView_ProjectionViewCanBeRetreived()
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
			var dataStore = new BlobStorageDataStore<TestEvent>(logger, new BlobStorageDataStoreConnectionStringFactory(configurationManager, logger));

			var event1 = new TestEvent
			{
				Rsn = Guid.NewGuid(),
				Id = Guid.NewGuid(),
				CorrelationId = correlationIdHelper.GetCorrelationId(),
				Frameworks = new List<string> { "Test 1" },
				TimeStamp = DateTimeOffset.UtcNow
			};

			// Act

#if NET472
			dataStore.Add
#else
			await dataStore.AddAsync
#endif
				(event1);

			// Assert
			var timer = new Stopwatch();
			timer.Start();
			TestEvent view =
#if NET472
				dataStore.GetByName
#else
				await dataStore.GetByNameAsync
#endif
					 (event1.Rsn);
			timer.Stop();
			Console.WriteLine("Load operation took {0}", timer.Elapsed);
			Assert.IsNotNull(view);
			Assert.AreEqual(event1.Id, view.Id);
		}
	}
}