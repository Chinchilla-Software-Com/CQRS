#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.StateManagement;

namespace Cqrs.Authentication
{
	public class DefaultAuthenticationTokenHelper
		: AuthenticationTokenHelper<SingleSignOnToken>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsn>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithCompanyRsn>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsnAndCompanyRsn>
		, IAuthenticationTokenHelper<int>
		, IAuthenticationTokenHelper<Guid>
		, IAuthenticationTokenHelper<string>
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

		public Guid SetAuthenticationToken(Guid token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		public int SetAuthenticationToken(int token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		public string SetAuthenticationToken(string token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		SingleSignOnTokenWithCompanyRsn IAuthenticationTokenHelper<SingleSignOnTokenWithCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<SingleSignOnTokenWithCompanyRsn>(CallContextPermissionScopeValueKey);
		}

		SingleSignOnTokenWithUserRsnAndCompanyRsn IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsnAndCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<SingleSignOnTokenWithUserRsnAndCompanyRsn>(CallContextPermissionScopeValueKey);
		}

		#region Implementation of IAuthenticationTokenHelper<int>

		int IAuthenticationTokenHelper<int>.GetAuthenticationToken()
		{
			return Cache.GetData<int>(CallContextPermissionScopeValueKey);
		}

		#endregion

		#region Implementation of IAuthenticationTokenHelper<Guid>

		Guid IAuthenticationTokenHelper<Guid>.GetAuthenticationToken()
		{
			return Cache.GetData<Guid>(CallContextPermissionScopeValueKey);
		}

		#endregion

		#region Implementation of IAuthenticationTokenHelper<string>

		string IAuthenticationTokenHelper<string>.GetAuthenticationToken()
		{
			return Cache.GetData<string>(CallContextPermissionScopeValueKey);
		}

		#endregion
	}
}