#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Configuration;
using System.Runtime.Remoting.Messaging;
using Chinchilla.StateManagement;
using Chinchilla.StateManagement.Threaded;
using Cqrs.MongoDB.Factories;

namespace Cqrs.MongoDB.Tests.Integration
{
	/// <summary>
	/// A <see cref="IMongoDbDataStoreConnectionStringFactory"/>
	/// that enables you to set a database name with <see cref="DatabaseName"/>. This means you can randomly generate your own database name per test.
	/// </summary>
	public class TestMongoDataStoreConnectionStringFactory : IMongoDbDataStoreConnectionStringFactory
	{
		private const string MongoDbConnectionStringKey = "MongoDb-Test";

		private const string CallContextDatabaseNameKey = "MongoDataStoreConnectionStringFactory¿DatabaseName";

		private static IContextItemCollection Query { get; set; }

		static TestMongoDataStoreConnectionStringFactory()
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

		#region Implementation of IMongoDbDataStoreConnectionStringFactory

		/// <summary>
		/// Gets the current connection string named "MongoDb-Test"
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
	}
}