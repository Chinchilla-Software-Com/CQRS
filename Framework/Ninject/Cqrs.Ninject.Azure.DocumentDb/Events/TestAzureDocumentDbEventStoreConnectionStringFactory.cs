#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Remoting.Messaging;
using Cqrs.Azure.DocumentDb.Events;
using Cqrs.Logging;

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

		public TestAzureDocumentDbEventStoreConnectionStringFactory(ILog logger)
			: base(logger)
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