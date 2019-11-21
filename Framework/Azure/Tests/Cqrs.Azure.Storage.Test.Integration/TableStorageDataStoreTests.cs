#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Chinchilla.Logging;
using Cqrs.Azure.BlobStorage.DataStores;
using Cqrs.Configuration;
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

		/// <summary>
		/// Create a <see cref="TableStorageDataStore{TData}"/> ready for testing.
		/// </summary>
		protected override TableStorageDataStore<TData> CreateDataStore<TData>(ILogger logger, IConfigurationManager configurationManager)
		{
			return new DataStores.TableStorageDataStore<TData>(logger, new DataStores.TableStorageDataStoreConnectionStringFactory<TData>(configurationManager, logger));
		}

		#endregion
	}
}