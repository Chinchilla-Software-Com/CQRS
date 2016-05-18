#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Runtime.Remoting.Messaging;

namespace Cqrs.Authentication
{
	public class SingleSignOnTokenValueHelper : IAuthenticationTokenHelper<ISingleSignOnToken>
	{
		private const string CallContextPermissionScopeValueKey = "SingleSignOnTokenValue";

		#region Implementation of IAuthenticationTokenHelper<out ISingleSignOnToken>

		public ISingleSignOnToken GetAuthenticationToken()
		{
			return (ISingleSignOnToken)CallContext.GetData(CallContextPermissionScopeValueKey);
		}

		public ISingleSignOnToken SetAuthenticationToken(ISingleSignOnToken value)
		{
			CallContext.SetData(CallContextPermissionScopeValueKey, value);
			return value;
		}

		#endregion
	}
}