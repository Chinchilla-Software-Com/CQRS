#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Azure.DocumentDb.Events;
using cdmdotnet.Logging;
using cdmdotnet.StateManagement;
using cdmdotnet.StateManagement.Threaded;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Events;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Ninject.Azure.DocumentDb.Events
{
	/// <summary>
	/// A <see cref="AzureDocumentDbEventStoreConnectionStringFactory"/>
	/// that enables you to set a database name with <see cref="DatabaseName"/>. This means you can randomly generate your own database name per test.
	/// </summary>
	public class TestAzureDocumentDbEventStoreConnectionStringFactory
		: AzureDocumentDbEventStoreConnectionStringFactory
		, IAzureDocumentDbSnapshotStoreConnectionStringFactory
	{
		private const string CallContextDatabaseNameKey = "AzureDocumentDbEventStoreConnectionStringFactory¿DatabaseName";

		private static IContextItemCollection Query { get; set; }

		static TestAzureDocumentDbEventStoreConnectionStringFactory()
		{
			Query = new ThreadedContextItemCollection();
		}

		/// <summary>
		/// The name of the database currently being used.
		/// </summary>
		public static string DatabaseName
		{
			get
			{
				return Query.GetData<string>(CallContextDatabaseNameKey);
			}
			set
			{
				Query.SetData(CallContextDatabaseNameKey, value);
			}
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="TestAzureDocumentDbEventStoreConnectionStringFactory"/> defaulting to using <see cref="ConfigurationManager"/>
		/// </summary>
		public TestAzureDocumentDbEventStoreConnectionStringFactory(ILogger logger)
			: base(logger, new ConfigurationManager())
		{
		}

		#region Implementation of IAzureDocumentDbDataStoreConnectionStringFactory

		/// <summary>
		/// Gets the value of <see cref="DatabaseName"/>.
		/// </summary>
		public override string GetEventStoreConnectionDatabaseName()
		{
			return DatabaseName;
		}

		#endregion

		#region Implementation of IAzureDocumentDbSnapshotStoreConnectionStringFactory

		/// <summary>
		/// Gets the current <see cref="DocumentClient"/>.
		/// </summary>
		public DocumentClient GetSnapshotStoreConnectionClient()
		{
			return GetEventStoreConnectionClient();
		}

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		public string GetSnapshotStoreConnectionDatabaseName()
		{
			return GetEventStoreConnectionDatabaseName();
		}

		/// <summary>
		/// Gets the current collection name.
		/// </summary>
		public string GetSnapshotStoreConnectionCollectionName()
		{
			return GetEventStoreConnectionCollectionName();
		}

		#endregion
	}
}