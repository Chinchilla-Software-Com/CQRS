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
	public class TestAzureDocumentDbEventStoreConnectionHelper : AzureDocumentDbEventStoreConnectionHelper
	{
		private const string CallContextDatabaseNameKey = "AzureDocumentDbEventStoreConnectionHelper¿DatabaseName";

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

		public TestAzureDocumentDbEventStoreConnectionHelper(ILog logger)
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