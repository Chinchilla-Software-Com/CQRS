using cdmdotnet.Logging;
using Cqrs.Azure.BlobStorage.DataStores;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.Azure.Storage.Test.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="TableStorageDataStore{TData}"/> class
	/// </summary>
	[TestClass]
	public class TableStorageDataStoreTests : BlobStorage.Test.Integration.TableStorageDataStoreTests
	{
		#region Overrides of TableStorageDataStoreTests

		protected override TableStorageDataStore<TData> CreateDataStore<TData>(ILogger logger, ITableStorageDataStoreConnectionStringFactory tableStorageDataStoreConnectionStringFactory)
		{
			return new DataStores.TableStorageDataStore<TData>(logger, tableStorageDataStoreConnectionStringFactory);
		}

		#endregion
	}
}