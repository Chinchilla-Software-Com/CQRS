#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Configuration;
using System.Runtime.Remoting.Messaging;
using cdmdotnet.StateManagement;
using cdmdotnet.StateManagement.Threaded;
using Cqrs.MongoDB.Events;

namespace Cqrs.MongoDB.Tests.Integration
{
	/// <summary>
	/// A <see cref="IMongoDbEventStoreConnectionStringFactory"/>
	/// that enables you to set a database name with <see cref="DatabaseName"/>. This means you can randomly generate your own database name per test.
	/// </summary>
	public class TestMongoEventStoreConnectionStringFactory : IMongoDbEventStoreConnectionStringFactory
	{
		internal const string MongoDbConnectionStringKey = "MongoDb-Test";

		internal const string CallContextDatabaseNameKey = "MongoEventStoreConnectionStringFactory¿DatabaseName";

		private static IContextItemCollection Query { get; set; }

		static TestMongoEventStoreConnectionStringFactory()
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

		#region Implementation of IMongoDbEventStoreConnectionStringFactory

		/// <summary>
		/// Gets the current connection string named "MongoDb-Test"
		/// </summary>
		public string GetEventStoreConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
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