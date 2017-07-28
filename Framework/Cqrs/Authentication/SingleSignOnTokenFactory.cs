#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Authentication
{
	/// <summary>
	/// A factory for creating new authentication tokens of type <typeparamref name="TSingleSignOnToken"/>.
	/// </summary>
	/// <typeparam name="TSingleSignOnToken">The <see cref="Type"/> of <see cref="ISingleSignOnToken"/>.</typeparam>
	public class SingleSignOnTokenFactory<TSingleSignOnToken> : ISingleSignOnTokenFactory<TSingleSignOnToken>
			where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		/// <summary>
		/// Create a new <typeparamref name="TSingleSignOnToken"/>.
		/// </summary>
		/// <param name="timeoutInMinutes">The amount of time in minutes to set the <see cref="ISingleSignOnToken.TimeOfExpiry"/> to. This is from <see cref="DateTime.UtcNow"/></param>
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

		/// <summary>
		/// Renew the value of <see cref="ISingleSignOnToken.TimeOfExpiry"/>.
		/// </summary>
		/// <param name="token">The <see cref="ISingleSignOnToken"/> to renew.</param>
		/// <param name="timeoutInMinutes">The amount of time in minutes to set the <see cref="ISingleSignOnToken.TimeOfExpiry"/> to. This is from <see cref="DateTime.UtcNow"/></param>
		public virtual TSingleSignOnToken RenewTokenExpiry(TSingleSignOnToken token, int timeoutInMinutes = 360)
		{
			token.TimeOfExpiry = DateTime.UtcNow.AddMinutes(timeoutInMinutes);

			return token;
		}
	}
}