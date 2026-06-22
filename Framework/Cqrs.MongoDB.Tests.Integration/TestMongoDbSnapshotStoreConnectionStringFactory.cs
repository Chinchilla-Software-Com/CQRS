#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Configuration;
using Chinchilla.StateManagement;
using Chinchilla.StateManagement.Threaded;
using Cqrs.MongoDB.Events;

namespace Cqrs.MongoDB.Tests.Integration
{
	/// <summary>
	/// A <see cref="IMongoDbEventStoreConnectionStringFactory"/>
	/// that enables you to set a database name with <see cref="DatabaseName"/>. This means you can randomly generate your own database name per test.
	/// </summary>
	public class TestMongoDbSnapshotStoreConnectionStringFactory : IMongoDbSnapshotStoreConnectionStringFactory
	{
		private static IContextItemCollection Query { get; set; }

		static TestMongoDbSnapshotStoreConnectionStringFactory()
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
				return Query.GetData<string>(TestMongoEventStoreConnectionStringFactory.CallContextDatabaseNameKey);
			}
			set
			{
				Query.SetData(TestMongoEventStoreConnectionStringFactory.CallContextDatabaseNameKey, value);
			}
		}

		#region Implementation of IMongoDbSnapshotStoreConnectionStringFactory

		/// <summary>
		/// Gets the current connection string.
		/// </summary>
		public string GetSnapshotStoreConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[TestMongoEventStoreConnectionStringFactory.MongoDbConnectionStringKey].ConnectionString;
		}

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		public string GetSnapshotStoreDatabaseName()
		{
			return DatabaseName;
		}

		#endregion
	}
}