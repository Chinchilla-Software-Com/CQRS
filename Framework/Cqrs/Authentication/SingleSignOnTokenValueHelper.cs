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
	public class SingleSignOnTokenValueHelper
		: IAuthenticationTokenHelper<ISingleSignOnToken>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsn>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithCompanyRsn>
		, IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsnAndCompanyRsn>
	{
		private const string CallContextPermissionScopeValueKey = "SingleSignOnTokenValue";

		#region Implementation of IAuthenticationTokenHelper<out ISingleSignOnToken>

		public ISingleSignOnToken GetAuthenticationToken()
		{
			return (ISingleSignOnToken)CallContext.GetData(CallContextPermissionScopeValueKey);
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
			CallContext.SetData(CallContextPermissionScopeValueKey, token);
			return token;
		}

		#endregion

		ISingleSignOnTokenWithUserRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsn>.GetAuthenticationToken()
		{
			return (ISingleSignOnTokenWithUserRsn)CallContext.GetData(CallContextPermissionScopeValueKey);
		}

		ISingleSignOnTokenWithCompanyRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithCompanyRsn>.GetAuthenticationToken()
		{
			return (ISingleSignOnTokenWithCompanyRsn)CallContext.GetData(CallContextPermissionScopeValueKey);
		}

		ISingleSignOnTokenWithUserRsnAndCompanyRsn IAuthenticationTokenHelper<ISingleSignOnTokenWithUserRsnAndCompanyRsn>.GetAuthenticationToken()
		{
			return (ISingleSignOnTokenWithUserRsnAndCompanyRsn)CallContext.GetData(CallContextPermissionScopeValueKey);
		}
	}
}