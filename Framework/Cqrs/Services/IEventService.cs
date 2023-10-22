#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Services
{
	/// <summary>
	/// A WCF <see cref="ServiceContractAttribute">ServiceContract</see> that provides read-only access to <see cref="IEventStore{TAuthenticationToken}"/> <see cref="IEvent{TAuthenticationToken}">events</see>
	/// raised with the same <see cref="IMessage.CorrelationId"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	[ServiceContract(Namespace = "https://getcqrs.net/Domain/1001/")]
	public interface IEventService<TAuthenticationToken>
	{
		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}">events</see>
		/// raised with the same <see cref="IMessage.CorrelationId"/>.
		/// </summary>
		/// <param name="serviceRequest">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}">events</see> to find.</param>
		[OperationContract]
#if NET40
		IServiceResponseWithResultData<IEnumerable<EventData>> GetEventData
#else
		Task<IServiceResponseWithResultData<IEnumerable<EventData>>> GetEventDataAsync
#endif
			(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest);
	}
}