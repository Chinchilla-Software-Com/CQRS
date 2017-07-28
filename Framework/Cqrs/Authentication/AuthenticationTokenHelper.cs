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
	/// <see cref="ISingleSignOnToken"/>, <see cref="ISingleSignOnTokenWithUserRsn"/>, <see cref="ISingleSignOnTokenWithCompanyRsn"/>
	/// or <see cref="ISingleSignOnTokenWithUserRsnAndCompanyRsn"/>.
	/// </summary>
	public class AuthenticationTokenHelper
	: AuthenticationTokenHelper<ISingleSignOnToken>
	, IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsn>
	, IAuthenticationTokenHelper<ISingleSignOnTokenWithCompanyRsn>
	, IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsnAndCompanyRsn>
	{
		private const string CallContextPermissionScopeValueKey = "SingleSignOnTokenValue";

		/// <summary>
		/// Instantiate a new instance of <see cref="AuthenticationTokenHelper"/>
		/// </summary>
		public AuthenticationTokenHelper(IContextItemCollectionFactory factory)
			: base(factory)
		{
			CacheKey = CallContextPermissionScopeValueKey;
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public ISingleSignOnTokenWithUserRsnAndCompanyRsn SetAuthenticationToken(ISingleSignOnTokenWithUserRsnAndCompanyRsn token)
		{
			SetAuthenticationToken((ISingleSignOnToken)token);
			return token;
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public ISingleSignOnTokenWithCompanyRsn SetAuthenticationToken(ISingleSignOnTokenWithCompanyRsn token)
		{
			SetAuthenticationToken((ISingleSignOnToken)token);
			return token;
		}

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public ISingleSignOnTokenWithUserRsn SetAuthenticationToken(ISingleSignOnTokenWithUserRsn token)
		{
			SetAuthenticationToken((ISingleSignOnToken)token);
			return token;
		}

		/// <summary>
		/// Get the current <see cref="ISingleSignOnTokenWithUserRsn">authentication token</see> for the current context/request.
		/// </summary>
		ISingleSignOnTokenWithUserRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<ISingleSignOnTokenWithUserRsn>(CallContextPermissionScopeValueKey);
		}

		/// <summary>
		/// Get the current <see cref="ISingleSignOnTokenWithCompanyRsn">authentication token</see> for the current context/request.
		/// </summary>
		ISingleSignOnTokenWithCompanyRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<ISingleSignOnTokenWithCompanyRsn>(CallContextPermissionScopeValueKey);
		}

		/// <summary>
		/// Get the current <see cref="ISingleSignOnTokenWithUserRsnAndCompanyRsn">authentication token</see> for the current context/request.
		/// </summary>
		ISingleSignOnTokenWithUserRsnAndCompanyRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsnAndCompanyRsn>.GetAuthenticationToken()
		{
			return Cache.GetData<ISingleSignOnTokenWithUserRsnAndCompanyRsn>(CallContextPermissionScopeValueKey);
		}
	}

	/// <summary>
	/// A helper for setting and retrieving authentication tokens of type <typeparamref name="TAuthenticationToken"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public class AuthenticationTokenHelper<TAuthenticationToken>
		: IAuthenticationTokenHelper<TAuthenticationToken>
	{
		/// <summary>
		/// The key used to store the authentication token in the <see cref="Cache"/>.
		/// </summary>
		protected string CacheKey = string.Format("{0}AuthenticationToken", typeof(TAuthenticationToken).FullName);

		/// <summary>
		/// Get or set the Cache.
		/// </summary>
		protected IContextItemCollection Cache { get; private set; }


		/// <summary>
		/// Instantiate a new instance of <see cref="AuthenticationTokenHelper{TAuthenticationToken}"/>
		/// </summary>
		public AuthenticationTokenHelper(IContextItemCollectionFactory factory)
		{
			Cache = factory.GetCurrentContext();
		}

		#region Implementation of IAuthenticationTokenHelper<out TAuthenticationToken>

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		public TAuthenticationToken SetAuthenticationToken(TAuthenticationToken token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		/// <summary>
		/// Get the current <typeparamref name="TAuthenticationToken">authentication token</typeparamref> for the current context/request.
		/// </summary>
		public TAuthenticationToken GetAuthenticationToken()
		{
			return Cache.GetData<TAuthenticationToken>(CacheKey);
		}

		#endregion
	}
}