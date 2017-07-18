#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;
using Cqrs.Ninject.Azure.Wcf;

namespace Cqrs.Ninject.Azure.WebJobs
{
	/// <summary>
	/// Execute command and event handlers in an Azure WebJob using Ninject
	/// </summary>
	public class CqrsNinjectJobHost<TAuthenticationToken, TAuthenticationTokenHelper> : CqrsWebHost<TAuthenticationToken, TAuthenticationTokenHelper>
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
	{
	}
}