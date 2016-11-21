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
	public class WebSingleSignOnTokenValueHelper
		: IAuthenticationTokenHelper<ISingleSignOnToken>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsn>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithCompanyRsn>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsnAndCompanyRsn>
	{
		private const string CallContextPermissionScopeValueKey = "SingleSignOnTokenValue";

		#region Implementation of IAuthenticationTokenHelper<out ISingleSignOnToken>

		public ISingleSignOnToken GetAuthenticationToken()
		{
			return (ISingleSignOnToken)HttpContext.Current.Items[CallContextPermissionScopeValueKey];
		}

		public ISingleSignOnTokenWithUserRsnAndCompanyRsn SetAuthenticationToken(ISingleSignOnTokenWithUserRsnAndCompanyRsn token)
		{
			SetAuthenticationToken((ISingleSignOnToken)token);
			return token;
		}

		public ISingleSignOnTokenWithCompanyRsn SetAuthenticationToken(ISingleSignOnTokenWithCompanyRsn token)
		{
			SetAuthenticationToken((ISingleSignOnToken)token);
			return token;
		}

		public ISingleSignOnTokenWithUserRsn SetAuthenticationToken(ISingleSignOnTokenWithUserRsn token)
		{
			SetAuthenticationToken((ISingleSignOnToken)token);
			return token;
		}

		public ISingleSignOnToken SetAuthenticationToken(ISingleSignOnToken token)
		{
			HttpContext.Current.Items[CallContextPermissionScopeValueKey] = token;
			return token;
		}

		#endregion

		ISingleSignOnTokenWithUserRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsn>.GetAuthenticationToken()
		{
			return (ISingleSignOnTokenWithUserRsn)HttpContext.Current.Items[CallContextPermissionScopeValueKey];
		}

		ISingleSignOnTokenWithCompanyRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithCompanyRsn>.GetAuthenticationToken()
		{
			return (ISingleSignOnTokenWithCompanyRsn)HttpContext.Current.Items[CallContextPermissionScopeValueKey];
		}

		ISingleSignOnTokenWithUserRsnAndCompanyRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsnAndCompanyRsn>.GetAuthenticationToken()
		{
			return (ISingleSignOnTokenWithUserRsnAndCompanyRsn)HttpContext.Current.Items[CallContextPermissionScopeValueKey];
		}
	}
}