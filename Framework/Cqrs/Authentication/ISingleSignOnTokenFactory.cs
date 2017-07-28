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
	/// A factory for creating new authentication tokens of type <typeparamref name="TSingleSignOnToken"/>.
	/// </summary>
	/// <typeparam name="TSingleSignOnToken">The <see cref="Type"/> of <see cref="ISingleSignOnToken"/>.</typeparam>
	[ServiceContract(Namespace = "https://getcqrs.net/SingleSignOn/TokenFactory")]
	public interface ISingleSignOnTokenFactory<TSingleSignOnToken>
			where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		/// <summary>
		/// Create a new <typeparamref name="TSingleSignOnToken"/>.
		/// </summary>
		/// <param name="timeoutInMinutes">The amount of time in minutes to set the <see cref="ISingleSignOnToken.TimeOfExpiry"/> to. This is from <see cref="DateTime.UtcNow"/></param>
		[OperationContract]
		TSingleSignOnToken CreateNew(int timeoutInMinutes = 360);

		/// <summary>
		/// Renew the value of <see cref="ISingleSignOnToken.TimeOfExpiry"/>.
		/// </summary>
		/// <param name="token">The <see cref="ISingleSignOnToken"/> to renew.</param>
		/// <param name="timeoutInMinutes">The amount of time in minutes to set the <see cref="ISingleSignOnToken.TimeOfExpiry"/> to. This is from <see cref="DateTime.UtcNow"/></param>
		[OperationContract]
		TSingleSignOnToken RenewTokenExpiry(TSingleSignOnToken token, int timeoutInMinutes = 360);
	}
}