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
	/// A Factory for creating new authentication tokens of type <see cref="SingleSignOnToken"/>
	/// </summary>
	public class DefaultSingleSignOnTokenFactory : SingleSignOnTokenFactory<SingleSignOnToken>, IDefaultSingleSignOnTokenFactory
	{
		#region Implementation of IDefaultSingleSignOnTokenFactory

		/// <summary>
		/// Renew the value of <see cref="ISingleSignOnToken.TimeOfExpiry"/>.
		/// </summary>
		/// <param name="token">The <see cref="ISingleSignOnToken"/> to renew.</param>
		/// <param name="timeoutInMinutes">The amount of time in minutes to set the <see cref="ISingleSignOnToken.TimeOfExpiry"/> to. This is from <see cref="DateTime.UtcNow"/>.</param>
#pragma warning disable 1066 // I like the explicit nature of informing what the default value should be of any implementation if not provided.
		ISingleSignOnToken IDefaultSingleSignOnTokenFactory.RenewTokenExpiry(ISingleSignOnToken token, int timeoutInMinutes = 360)
#pragma warning restore 1066
		{
			return RenewTokenExpiry(token, timeoutInMinutes);
		}

		#endregion

		/// <summary>
		/// Renew the value of <see cref="ISingleSignOnToken.TimeOfExpiry"/>.
		/// </summary>
		/// <param name="token">The <see cref="ISingleSignOnToken"/> to renew.</param>
		/// <param name="timeoutInMinutes">The amount of time in minutes to set the <see cref="ISingleSignOnToken.TimeOfExpiry"/> to. This is from <see cref="DateTime.UtcNow"/>.</param>
		public virtual TSingleSignOnToken RenewTokenExpiry<TSingleSignOnToken>(TSingleSignOnToken token, int timeoutInMinutes = 360)
			where TSingleSignOnToken : ISingleSignOnToken
		{
			token.TimeOfExpiry = DateTime.UtcNow.AddMinutes(timeoutInMinutes);

			return token;
		}
	}
}