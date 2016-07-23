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
	public class SingleSignOnTokenFactory : ISingleSignOnTokenFactory
	{
		public ISingleSignOnToken CreateNew(int timeoutInMinutes = 360)
		{
			return CreateNew<SingleSignOnToken>();
		}

		public ISingleSignOnToken CreateNew<TSingleSignOnToken>(int timeoutInMinutes = 360)
			where TSingleSignOnToken : ISingleSignOnToken, new()
		{
			var token = new TSingleSignOnToken
			{
				Token = Guid.NewGuid().ToString("N"),
				DateIssued = DateTime.UtcNow,
				TimeOfExpiry = DateTime.UtcNow.AddMinutes(timeoutInMinutes),
			};

			return RenewTokenExpiry(token, timeoutInMinutes);
		}

		public ISingleSignOnToken RenewTokenExpiry(ISingleSignOnToken token, int timeoutInMinutes = 360)
		{
			token.TimeOfExpiry = DateTime.UtcNow.AddMinutes(timeoutInMinutes);

			return token;
		}
	}
}