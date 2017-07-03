#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Remoting.Messaging;
using Cqrs.Azure.DocumentDb.Factories;
using cdmdotnet.Logging;
using Cqrs.Configuration;

namespace Cqrs.Ninject.Azure.DocumentDb.Factories
{
	public class TestAzureDocumentDbDataStoreConnectionStringFactory : AzureDocumentDbDataStoreConnectionStringFactory
	{
		private const string CallContextDatabaseNameKey = "AzureDocumentDbDataStoreConnectionStringFactory¿DatabaseName";

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

		public TestAzureDocumentDbDataStoreConnectionStringFactory(ILogger logger)
			: base(logger, new ConfigurationManager())
		{
		}

		#region Implementation of IAzureDocumentDbDataStoreConnectionStringFactory

		public override string GetAzureDocumentDbDatabaseName()
		{
			return DatabaseName;
		}

		#region Overrides of AzureDocumentDbDataStoreConnectionStringFactory

		public override bool UseOneCollectionPerDataStore()
		{
			return true;
		}

		#endregion

		#endregion
	}
}