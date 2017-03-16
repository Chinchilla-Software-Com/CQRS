using System;
using cdmdotnet.Logging;
using Cqrs.Azure.BlobStorage;
using Cqrs.Azure.BlobStorage.Events;
using Cqrs.Events;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.Azure.Storage.Test.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="TableStorageEventStore{TAuthenticationToken}"/> class
	/// </summary>
	[TestClass]
	public class TableStorageEventStoreTests : BlobStorage.Test.Integration.TableStorageEventStoreTests
	{
		#region Overrides of TableStorageDataStoreTests

		protected override TableStorageEventStore<Guid> CreateDataStore(IEventBuilder<Guid> eventBuilder, IEventDeserialiser<Guid> eventDeserialiser, ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory)
		{
			return new Events.TableStorageEventStore<Guid>(eventBuilder, eventDeserialiser, logger, tableStorageEventStoreConnectionStringFactory);
		}

		#endregion

	}
}