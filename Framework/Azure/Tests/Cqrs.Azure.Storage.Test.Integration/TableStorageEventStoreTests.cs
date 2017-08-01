#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

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

		/// <summary>
		/// Create a <see cref="TableStorageEventStore{TAuthenticationToken}"/> ready for testing.
		/// </summary>
		protected override TableStorageEventStore<Guid> CreateEventStore(IEventBuilder<Guid> eventBuilder, IEventDeserialiser<Guid> eventDeserialiser, ILogger logger, ITableStorageStoreConnectionStringFactory tableStorageEventStoreConnectionStringFactory)
		{
			return new Events.TableStorageEventStore<Guid>(eventBuilder, eventDeserialiser, logger, tableStorageEventStoreConnectionStringFactory);
		}

		#endregion
	}
}