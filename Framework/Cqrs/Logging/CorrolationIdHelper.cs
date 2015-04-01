#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Remoting.Messaging;

namespace Cqrs.Logging
{
	public class CorrolationIdHelper : ICorrolationIdHelper
	{
		private const string CallContextPermissoinScopeValueKey = "CorrolationIdValue";

		#region Implementation of ICorrolationIdHelper

		public Guid GetCorrolationId()
		{
			return (Guid)CallContext.GetData(CallContextPermissoinScopeValueKey);
		}

		public Guid SetCorrolationId(Guid corrolationId)
		{
			CallContext.SetData(CallContextPermissoinScopeValueKey, corrolationId);
			return corrolationId;
		}

		#endregion
	}
}