#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Remoting.Messaging;
using Cqrs.Azure.DocumentDb.Factories;
using Cqrs.Logging;

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

		public TestAzureDocumentDbDataStoreConnectionStringFactory(ILog logger)
			: base(logger)
		{
		}

		#region Implementation of IAzureDocumentDbDataStoreConnectionStringFactory

		public override string GetAzureDocumentDbDatabaseName()
		{
			return DatabaseName;
		}

		#endregion
	}
}