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
using Cqrs.Events;

namespace Cqrs.Services
{
	[ServiceContract(Namespace = "https://getcqrs.net/Domain/1001/")]
	public interface IEventService<TAuthenticationToken>
	{
		[OperationContract]
		IServiceResponseWithResultData<IEnumerable<EventData>> GetEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest);
	}
}