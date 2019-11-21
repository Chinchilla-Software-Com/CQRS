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
using Cqrs.Mongo.Factories;

namespace Cqrs.Ninject.Mongo
{
	/// <summary>
	/// A <see cref="IMongoDataStoreConnectionStringFactory"/>
	/// that enables you to set a database name with <see cref="DatabaseName"/>. This means you can randomly generate your own database name per test.
	/// </summary>
	public class TestMongoDataStoreConnectionStringFactory : IMongoDataStoreConnectionStringFactory
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
		/// <summary>
		/// Gets the current connection string.
		/// </summary>
		public string GetMongoConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
		}

		/// <summary>
		/// Gets the value of <see cref="DatabaseName"/>.
		/// </summary>
		public string GetMongoDatabaseName()
		{
			return DatabaseName;
		}
	}
}