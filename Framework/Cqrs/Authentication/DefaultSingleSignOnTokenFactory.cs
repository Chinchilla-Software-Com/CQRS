#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Authentication
{
	public class DefaultSingleSignOnTokenFactory : SingleSignOnTokenFactory<SingleSignOnToken>, IDefaultSingleSignOnTokenFactory
	{
		#region Implementation of IDefaultSingleSignOnTokenFactory

		ISingleSignOnToken IDefaultSingleSignOnTokenFactory.RenewTokenExpiry(ISingleSignOnToken token, int timeoutInMinutes = 360)
		{
			return RenewTokenExpiry(token, timeoutInMinutes);
		}

		#endregion

		public virtual TSingleSignOnToken RenewTokenExpiry<TSingleSignOnToken>(TSingleSignOnToken token, int timeoutInMinutes = 360)
			where TSingleSignOnToken : ISingleSignOnToken
		{
			token.TimeOfExpiry = DateTime.UtcNow.AddMinutes(timeoutInMinutes);

			return token;
		}
	}
}