#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Web;

namespace Cqrs.Authentication
{
	public class WebSingleSignOnTokenValueHelper : IAuthenticationTokenHelper<ISingleSignOnToken>
	{
		private const string CallContextPermissionScopeValueKey = "SingleSignOnTokenValue";

		#region Implementation of IAuthenticationTokenHelper<out ISingleSignOnToken>

		public ISingleSignOnToken GetAuthenticationToken()
		{
			return (ISingleSignOnToken)HttpContext.Current.Items[CallContextPermissionScopeValueKey];
		}

		public ISingleSignOnToken SetAuthenticationToken(ISingleSignOnToken value)
		{
			HttpContext.Current.Items[CallContextPermissionScopeValueKey] = value;
			return value;
		}

		#endregion
	}
}