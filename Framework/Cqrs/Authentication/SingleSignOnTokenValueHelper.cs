#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.StateManagement;

namespace Cqrs.Authentication
{
	public class AuthenticationTokenHelper
		: AuthenticationTokenHelper<ISingleSignOnToken>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsn>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithCompanyRsn>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsnAndCompanyRsn>
	{
		private const string CallContextPermissionScopeValueKey = "SingleSignOnTokenValue";

		public AuthenticationTokenHelper(IContextItemCollectionFactory factory)
			: base(factory)
		{
			CacheKey = CallContextPermissionScopeValueKey;
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

		ISingleSignOnTokenWithUserRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<ISingleSignOnTokenWithUserRsn>(CallContextPermissionScopeValueKey);
		}

		ISingleSignOnTokenWithCompanyRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<ISingleSignOnTokenWithCompanyRsn>(CallContextPermissionScopeValueKey);
		}

		ISingleSignOnTokenWithUserRsnAndCompanyRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsnAndCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<ISingleSignOnTokenWithUserRsnAndCompanyRsn>(CallContextPermissionScopeValueKey);
		}
	}
}