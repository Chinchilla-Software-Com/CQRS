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
	/// <summary>
	/// A helper for setting and retrieving authentication tokens of type 
	/// <see cref="SingleSignOnToken"/>, <see cref="SingleSignOnTokenWithUserRsn"/>, <see cref="SingleSignOnTokenWithCompanyRsn"/>, <see cref="SingleSignOnTokenWithUserRsnAndCompanyRsn"/>
	/// <see cref="int"/>, <see cref="Guid"/> or <see cref="string"/>.
	/// </summary>
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

		/// <summary>
		/// Instantiate a new instance of <see cref="DefaultAuthenticationTokenHelper"/>
		/// </summary>
		public DefaultAuthenticationTokenHelper(IContextItemCollectionFactory factory)
			: base(factory)
		{
			CacheKey = CallContextPermissionScopeValueKey;
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public SingleSignOnTokenWithUserRsnAndCompanyRsn SetAuthenticationToken(SingleSignOnTokenWithUserRsnAndCompanyRsn token)
		{
			SetAuthenticationToken((SingleSignOnToken)token);
			return token;
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public SingleSignOnTokenWithCompanyRsn SetAuthenticationToken(SingleSignOnTokenWithCompanyRsn token)
		{
			SetAuthenticationToken((SingleSignOnToken)token);
			return token;
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public SingleSignOnTokenWithUserRsn SetAuthenticationToken(SingleSignOnTokenWithUserRsn token)
		{
			SetAuthenticationToken((SingleSignOnToken)token);
			return token;
		}

		/// <summary>
		/// Get the current <see cref="SingleSignOnTokenWithUserRsn">authentication token</see> for the current context/request.
		/// </summary>
		SingleSignOnTokenWithUserRsn IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<SingleSignOnTokenWithUserRsn>(CallContextPermissionScopeValueKey);
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public Guid SetAuthenticationToken(Guid token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public int SetAuthenticationToken(int token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public string SetAuthenticationToken(string token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		/// <summary>
		/// Get the current <see cref="SingleSignOnTokenWithCompanyRsn">authentication token</see> for the current context/request.
		/// </summary>
		SingleSignOnTokenWithCompanyRsn IAuthenticationTokenHelper<SingleSignOnTokenWithCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<SingleSignOnTokenWithCompanyRsn>(CallContextPermissionScopeValueKey);
		}

		/// <summary>
		/// Get the current <see cref="SingleSignOnTokenWithUserRsnAndCompanyRsn">authentication token</see> for the current context/request.
		/// </summary>
		SingleSignOnTokenWithUserRsnAndCompanyRsn IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsnAndCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<SingleSignOnTokenWithUserRsnAndCompanyRsn>(CallContextPermissionScopeValueKey);
		}

		#region Implementation of IAuthenticationTokenHelper<int>

		/// <summary>
		/// Get the current <see cref="int">authentication token</see> for the current context/request.
		/// </summary>
		int IAuthenticationTokenHelper<int>.GetAuthenticationToken()
		{
			return Cache.GetData<int>(CallContextPermissionScopeValueKey);
		}

		#endregion

		#region Implementation of IAuthenticationTokenHelper<Guid>

		/// <summary>
		/// Get the current <see cref="Guid">authentication token</see> for the current context/request.
		/// </summary>
		Guid IAuthenticationTokenHelper<Guid>.GetAuthenticationToken()
		{
			return Cache.GetData<Guid>(CallContextPermissionScopeValueKey);
		}

		#endregion

		#region Implementation of IAuthenticationTokenHelper<string>

		/// <summary>
		/// Get the current <see cref="string">authentication token</see> for the current context/request.
		/// </summary>
		string IAuthenticationTokenHelper<string>.GetAuthenticationToken()
		{
			return Cache.GetData<string>(CallContextPermissionScopeValueKey);
		}

		#endregion
	}
}