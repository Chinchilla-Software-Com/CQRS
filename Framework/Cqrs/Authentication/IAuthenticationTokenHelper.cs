#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.ServiceModel;

namespace Cqrs.Authentication
{
	[ServiceContract(Namespace = "https://getcqrs.net/SingleSignOn/TokenHelper")]
	public interface IAuthenticationTokenHelper<TAuthenticationToken>
	{
		[OperationContract]
		TAuthenticationToken GetAuthenticationToken();

		[OperationContract]
		TAuthenticationToken SetAuthenticationToken(TAuthenticationToken token);
	}
}