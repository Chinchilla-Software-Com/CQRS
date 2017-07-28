#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ServiceModel;

namespace Cqrs.Authentication
{
	/// <summary>
	/// A Factory for creating new authentication tokens of type <see cref="SingleSignOnToken"/>
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/SingleSignOn/TokenFactory")]
	public interface IDefaultSingleSignOnTokenFactory : ISingleSignOnTokenFactory<SingleSignOnToken>
	{
		/// <summary>
		/// Renew the value of <see cref="ISingleSignOnToken.TimeOfExpiry"/>.
		/// </summary>
		/// <param name="token">The <see cref="ISingleSignOnToken"/> to renew.</param>
		/// <param name="timeoutInMinutes">The amount of time in minutes to set the <see cref="ISingleSignOnToken.TimeOfExpiry"/> to. This is from <see cref="DateTime.UtcNow"/></param>
		[OperationContract]
		ISingleSignOnToken RenewTokenExpiry(ISingleSignOnToken token, int timeoutInMinutes = 360);
	}
}