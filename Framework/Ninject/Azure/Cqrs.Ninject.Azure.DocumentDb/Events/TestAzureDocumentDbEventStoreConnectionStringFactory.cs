#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Remoting.Messaging;
using Cqrs.Azure.DocumentDb.Events;
using cdmdotnet.Logging;
using Cqrs.Configuration;

namespace Cqrs.Ninject.Azure.DocumentDb.Events
{
	public class TestAzureDocumentDbEventStoreConnectionStringFactory : AzureDocumentDbEventStoreConnectionStringFactory
	{
		private const string CallContextDatabaseNameKey = "AzureDocumentDbEventStoreConnectionStringFactory¿DatabaseName";

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

		public TestAzureDocumentDbEventStoreConnectionStringFactory(ILogger logger)
			: base(logger, new ConfigurationManager())
		{
		}

		#region Implementation of IAzureDocumentDbDataStoreConnectionStringFactory

		public override string GetEventStoreConnectionDatabaseName()
		{
			return DatabaseName;
		}

		#endregion
	}
}