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
	public class DefaultAuthenticationTokenHelper
		: AuthenticationTokenHelper<SingleSignOnToken>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsn>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithCompanyRsn>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsnAndCompanyRsn>
	{
		private const string CallContextPermissionScopeValueKey = "SingleSignOnTokenValue";

		public DefaultAuthenticationTokenHelper(IContextItemCollectionFactory factory)
			: base(factory)
		{
			CacheKey = CallContextPermissionScopeValueKey;
		}

		public SingleSignOnTokenWithUserRsnAndCompanyRsn SetAuthenticationToken(SingleSignOnTokenWithUserRsnAndCompanyRsn token)
		{
			SetAuthenticationToken((SingleSignOnToken)token);
			return token;
		}

		public SingleSignOnTokenWithCompanyRsn SetAuthenticationToken(SingleSignOnTokenWithCompanyRsn token)
		{
			SetAuthenticationToken((SingleSignOnToken)token);
			return token;
		}

		public SingleSignOnTokenWithUserRsn SetAuthenticationToken(SingleSignOnTokenWithUserRsn token)
		{
			SetAuthenticationToken((SingleSignOnToken)token);
			return token;
		}
	
		SingleSignOnTokenWithUserRsn IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<SingleSignOnTokenWithUserRsn>(CallContextPermissionScopeValueKey);
		}

		SingleSignOnTokenWithCompanyRsn IAuthenticationTokenHelper<SingleSignOnTokenWithCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<SingleSignOnTokenWithCompanyRsn>(CallContextPermissionScopeValueKey);
		}

		SingleSignOnTokenWithUserRsnAndCompanyRsn IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsnAndCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<SingleSignOnTokenWithUserRsnAndCompanyRsn>(CallContextPermissionScopeValueKey);
		}
	}
}