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
	[ServiceContract(Namespace = "https://getcqrs.net/SingleSignOn/TokenFactory")]
	public interface ISingleSignOnTokenFactory<TSingleSignOnToken>
			where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		[OperationContract]
		TSingleSignOnToken CreateNew(int timeoutInMinutes = 360);

		[OperationContract]
		TSingleSignOnToken RenewTokenExpiry(TSingleSignOnToken token, int timeoutInMinutes = 360);
	}
}