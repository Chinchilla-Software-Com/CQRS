#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using Cqrs.Events;

namespace Cqrs.Services
{
	/// <summary>
	/// Resolves <see cref="EventData"/>, <see cref="ServiceRequestWithData{TAuthenticationToken, Guid}" /> and <see cref="ServiceResponseWithResultData{IEnumerableEventData}"/> parameter types when serialising with WCF.
	/// </summary>
	public interface IEventDataResolver : IServiceParameterResolver
	{
	}
}
