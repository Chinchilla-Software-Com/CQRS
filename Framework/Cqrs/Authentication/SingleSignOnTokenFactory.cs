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
	public class SingleSignOnTokenFactory<TSingleSignOnToken> : ISingleSignOnTokenFactory<TSingleSignOnToken>
			where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		public virtual TSingleSignOnToken CreateNew(int timeoutInMinutes = 360)
		{
			var token = new TSingleSignOnToken
			{
				Token = Guid.NewGuid().ToString("N"),
				DateIssued = DateTime.UtcNow,
				TimeOfExpiry = DateTime.UtcNow.AddMinutes(timeoutInMinutes),
			};

			return RenewTokenExpiry(token, timeoutInMinutes);
		}

		public virtual TSingleSignOnToken RenewTokenExpiry(TSingleSignOnToken token, int timeoutInMinutes = 360)
		{
			token.TimeOfExpiry = DateTime.UtcNow.AddMinutes(timeoutInMinutes);

			return token;
		}
	}
}