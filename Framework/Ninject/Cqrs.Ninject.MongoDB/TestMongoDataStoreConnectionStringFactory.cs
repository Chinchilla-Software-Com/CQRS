#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Configuration;
using System.Runtime.Remoting.Messaging;
using Cqrs.MongoDB.Events;
using Cqrs.MongoDB.Factories;

namespace Cqrs.Ninject.MongoDB
{
	public class TestMongoDbDataStoreConnectionStringFactory
		: IMongoDbDataStoreConnectionStringFactory
		, IMongoDbEventStoreConnectionStringFactory
	{
		private const string MongoDbConnectionStringKey = "MongoDb-Test";

		private const string CallContextDatabaseNameKey = "MongoDataStoreConnectionStringFactory¿DatabaseName";

		public static string DatabaseName
		{
			get
			{
				return (string)CallContext.GetData(CallContextDatabaseNameKey);
			}
			set
			{
				CallContext.SetData(CallContextDatabaseNameKey, value);
			}
		}

		#region Implementation of IMongoDataStoreConnectionStringFactory

		public string GetDataStoreConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
		}

		public string GetDataStoreDatabaseName()
		{
			return DatabaseName;
		}

		#endregion

		#region Implementation of IMongoDbEventStoreConnectionStringFactory

		public string GetEventStoreConnectionString()
		{
			return GetDataStoreConnectionString();
		}

		public string GetEventStoreDatabaseName()
		{
			return GetDataStoreDatabaseName();
		}

		#endregion
	}
}