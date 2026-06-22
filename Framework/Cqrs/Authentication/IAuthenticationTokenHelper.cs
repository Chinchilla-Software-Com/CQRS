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
	/// A helper for setting and retrieving authentication tokens of type <typeparamref name="TAuthenticationToken"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	[ServiceContract(Namespace = "https://getcqrs.net/SingleSignOn/TokenHelper")]
	public interface IAuthenticationTokenHelper<TAuthenticationToken>
	{
		/// <summary>
		/// Get the current <typeparamref name="TAuthenticationToken">authentication token</typeparamref> for the current context/request.
		/// </summary>
		[OperationContract]
		TAuthenticationToken GetAuthenticationToken();

		/// <summary>
		/// Set the provided <paramref name="token"/> for the current context/request.
		/// </summary>
		[OperationContract]
		TAuthenticationToken SetAuthenticationToken(TAuthenticationToken token);
	}
}