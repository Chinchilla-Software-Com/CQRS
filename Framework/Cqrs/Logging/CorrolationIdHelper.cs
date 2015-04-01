#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Remoting.Messaging;

namespace Cqrs.Logging
{
	public class CorrolationIdHelper : ICorrolationIdHelper
	{
		private const string CallContextPermissoinScopeValueKey = "CorrolationIdValue";

		#region Implementation of ICorrolationIdHelper

		public string GetCorrolationId()
		{
			return (string)CallContext.GetData(CallContextPermissoinScopeValueKey);
		}

		public string SetCorrolationId(string corrolationId)
		{
			CallContext.SetData(CallContextPermissoinScopeValueKey, corrolationId);
			return corrolationId;
		}

		#endregion
	}
}