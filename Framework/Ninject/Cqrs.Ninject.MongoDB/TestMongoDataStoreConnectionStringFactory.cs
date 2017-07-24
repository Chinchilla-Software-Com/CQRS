#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Configuration;
using cdmdotnet.StateManagement;
using cdmdotnet.StateManagement.Threaded;
using Cqrs.DataStores;
using Cqrs.Events;
using Cqrs.MongoDB.Events;
using Cqrs.MongoDB.Factories;

namespace Cqrs.Ninject.MongoDB
{
	/// <summary>
	/// A <see cref="IMongoDbDataStoreConnectionStringFactory"/> and <see cref="IMongoDbEventStoreConnectionStringFactory"/>
	/// that enables you to set a database name with <see cref="DatabaseName"/>. This means you can randomly generate your own database name per test.
	/// Both <see cref="IEventStore{TAuthenticationToken}"/> and <see cref="IDataStore{TData}"/> use the same connection string and database name.
	/// </summary>
	public class TestMongoDbDataStoreConnectionStringFactory
		: IMongoDbDataStoreConnectionStringFactory
		, IMongoDbEventStoreConnectionStringFactory
	{
		private const string MongoDbConnectionStringKey = "MongoDb-Test";

		private const string CallContextDatabaseNameKey = "MongoDataStoreConnectionStringFactory¿DatabaseName";

		private static IContextItemCollection Query { get; set; }

		static TestMongoDbDataStoreConnectionStringFactory()
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

		#region Implementation of IMongoDataStoreConnectionStringFactory

		/// <summary>
		/// Gets the current connection string.
		/// </summary>
		public string GetDataStoreConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
		}

		/// <summary>
		/// Gets the value of <see cref="DatabaseName"/>.
		/// </summary>
		public string GetDataStoreDatabaseName()
		{
			return DatabaseName;
		}

		#endregion

		#region Implementation of IMongoDbEventStoreConnectionStringFactory

		/// <summary>
		/// Gets the current connection string.
		/// </summary>
		public string GetEventStoreConnectionString()
		{
			return GetDataStoreConnectionString();
		}

		/// <summary>
		/// Gets the value of <see cref="DatabaseName"/>.
		/// </summary>
		public string GetEventStoreDatabaseName()
		{
			return DatabaseName;
		}

		#endregion
	}
}