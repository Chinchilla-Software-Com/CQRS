#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Azure.DocumentDb.Factories;
using Chinchilla.Logging;
using Chinchilla.StateManagement;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Ninject.Azure.DocumentDb.Factories
{
	/// <summary>
	/// A <see cref="AzureDocumentDbDataStoreConnectionStringFactory"/>
	/// that enables you to set a database name with <see cref="DatabaseName"/>. This means you can randomly generate your own database name per test.
	/// </summary>
	public class TestAzureDocumentDbDataStoreConnectionStringFactory : AzureDocumentDbDataStoreConnectionStringFactory
	{
		private const string CallContextDatabaseNameKey = "AzureDocumentDbDataStoreConnectionStringFactory¿DatabaseName";

		private static IContextItemCollection Query { get; set; }

		static TestAzureDocumentDbDataStoreConnectionStringFactory()
		{
			Query = new Chinchilla.StateManagement.Threaded.ContextItemCollection();
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
		/// Instantiates a new instance of <see cref="TestAzureDocumentDbDataStoreConnectionStringFactory"/> defaulting to using <see cref="ConfigurationManager"/>
		/// </summary>
		public TestAzureDocumentDbDataStoreConnectionStringFactory(ILogger logger)
			: base(logger, new ConfigurationManager())
		{
		}

		#region Implementation of IAzureDocumentDbDataStoreConnectionStringFactory

		/// <summary>
		/// Gets the value of <see cref="DatabaseName"/>.
		/// </summary>
		public override string GetAzureDocumentDbDatabaseName()
		{
			return DatabaseName;
		}

		#endregion

		#region Overrides of AzureDocumentDbDataStoreConnectionStringFactory

		/// <summary>
		/// Indicates if a different collection should be used per <see cref="IEntity"/>/<see cref="IDataStore{TData}"/> or a single collection used for all instances of <see cref="IDataStore{TData}"/> and <see cref="IDataStore{TData}"/>.
		/// Setting this to true can become expensive as each <see cref="IEntity"/> will have it's own collection. Check the relevant SDK/pricing models.
		/// </summary>
		/// <returns>Always returns true.</returns>
		public override bool UseSingleCollectionForAllDataStores()
		{
			return true;
		}

		#endregion
	}
}