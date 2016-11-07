using System.Configuration;
using System.Runtime.Remoting.Messaging;
using Cqrs.MongoDB.Events;

namespace Cqrs.MongoDB.Tests.Integration
{
	public class TestMongoEventStoreConnectionStringFactory : IMongoDbEventStoreConnectionStringFactory
	{
		private const string MongoDbConnectionStringKey = "MongoDb-Test";

		private const string CallContextDatabaseNameKey = "MongoEventStoreConnectionStringFactory¿DatabaseName";

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

		#region Implementation of IMongoDbEventStoreConnectionStringFactory

		public string GetEventStoreConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[MongoDbConnectionStringKey].ConnectionString;
		}

		public string GetEventStoreDatabaseName()
		{
			return DatabaseName;
		}

		#endregion
	}
}