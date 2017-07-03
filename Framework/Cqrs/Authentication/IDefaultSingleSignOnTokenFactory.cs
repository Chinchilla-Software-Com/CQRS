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
	[ServiceContract(Namespace = "https://getcqrs.net/SingleSignOn/TokenFactory")]
	public interface IDefaultSingleSignOnTokenFactory : ISingleSignOnTokenFactory<SingleSignOnToken>
	{
		[OperationContract]
		ISingleSignOnToken RenewTokenExpiry(ISingleSignOnToken token, int timeoutInMinutes = 360);
	}
}