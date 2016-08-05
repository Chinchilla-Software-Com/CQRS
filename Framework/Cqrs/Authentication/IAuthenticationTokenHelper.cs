#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.ServiceModel;

namespace Cqrs.Authentication
{
	[ServiceContract(Namespace = "http://cqrs.co.nz/SingleSignOn/TokenHelper")]
	public interface IAuthenticationTokenHelper<TAuthenticationToken>
	{
		[OperationContract]
		TAuthenticationToken GetAuthenticationToken();

		[OperationContract]
		TAuthenticationToken SetAuthenticationToken(TAuthenticationToken token);
	}
}