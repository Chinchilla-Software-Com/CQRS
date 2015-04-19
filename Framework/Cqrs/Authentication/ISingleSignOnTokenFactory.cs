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
	public interface ISingleSignOnTokenFactory
	{
		[OperationContract]
		ISingleSignOnToken CreateNew(int timeoutInMinutes = 360);

		[OperationContract]
		ISingleSignOnToken RenewTokenExpiry(ISingleSignOnToken token, int timeoutInMinutes = 360);
	}
}