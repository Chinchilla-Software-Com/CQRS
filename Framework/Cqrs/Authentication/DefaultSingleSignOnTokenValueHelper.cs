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
	public class DefaultSingleSignOnTokenValueHelper
		: IAuthenticationTokenHelper<SingleSignOnToken>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsn>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithCompanyRsn>
		, IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsnAndCompanyRsn>
	{
		private const string CallContextPermissionScopeValueKey = "SingleSignOnTokenValue";

		#region Implementation of IAuthenticationTokenHelper<out SingleSignOnToken>

		public SingleSignOnToken GetAuthenticationToken()
		{
			return (SingleSignOnToken)CallContext.GetData(CallContextPermissionScopeValueKey);
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

		public SingleSignOnToken SetAuthenticationToken(SingleSignOnToken token)
		{
			CallContext.SetData(CallContextPermissionScopeValueKey, token);
			return token;
		}

		#endregion

		SingleSignOnTokenWithUserRsn IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsn>.GetAuthenticationToken()
		{
			return (SingleSignOnTokenWithUserRsn)CallContext.GetData(CallContextPermissionScopeValueKey);
		}

		SingleSignOnTokenWithCompanyRsn IAuthenticationTokenHelper<SingleSignOnTokenWithCompanyRsn>.GetAuthenticationToken()
		{
			return (SingleSignOnTokenWithCompanyRsn)CallContext.GetData(CallContextPermissionScopeValueKey);
		}

		SingleSignOnTokenWithUserRsnAndCompanyRsn IAuthenticationTokenHelper<SingleSignOnTokenWithUserRsnAndCompanyRsn>.GetAuthenticationToken()
		{
			return (SingleSignOnTokenWithUserRsnAndCompanyRsn)CallContext.GetData(CallContextPermissionScopeValueKey);
		}
	}
}