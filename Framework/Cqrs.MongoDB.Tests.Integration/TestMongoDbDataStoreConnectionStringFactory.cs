using System.Configuration;
using System.Runtime.Remoting.Messaging;
using Cqrs.MongoDB.Factories;

namespace Cqrs.MongoDB.Tests.Integration
{
	public class TestMongoDataStoreConnectionStringFactory : IMongoDbDataStoreConnectionStringFactory
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

		#region Implementation of IMongoDbDataStoreConnectionStringFactory

		public string GetDataStoreConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
		}

		public string GetDataStoreDatabaseName()
		{
			return DatabaseName;
		}

		#endregion
	}
}