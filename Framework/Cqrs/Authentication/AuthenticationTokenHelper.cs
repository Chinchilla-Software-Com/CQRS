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

	public class AuthenticationTokenHelper<TToken1>
		: IAuthenticationTokenHelper<TToken1>
	{
		protected string CacheKey = string.Format("{0}AuthenticationToken", typeof(TToken1).FullName);

		protected IContextItemCollection Cache { get; private set; }

		public AuthenticationTokenHelper(IContextItemCollectionFactory factory)
		{
			Cache = factory.GetCurrentContext();
		}

		#region Implementation of IAuthenticationTokenHelper<out TToken1>

		public TToken1 SetAuthenticationToken(TToken1 token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		public TToken1 GetAuthenticationToken()
		{
			return Cache.GetData<TToken1>(CacheKey);
		}

		#endregion
	}
}