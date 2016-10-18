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
	[ServiceContract(Namespace = "http://cqrs.co.nz/SingleSignOn/TokenFactory")]
	public interface IDefaultSingleSignOnTokenFactory : ISingleSignOnTokenFactory<SingleSignOnToken>
	{
		[OperationContract]
		ISingleSignOnToken RenewTokenExpiry(ISingleSignOnToken token, int timeoutInMinutes = 360);
	}
}