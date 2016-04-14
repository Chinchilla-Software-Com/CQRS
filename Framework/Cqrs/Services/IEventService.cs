using System;
using System.Collections.Generic;
using System.ServiceModel;
using Cqrs.Events;

namespace Cqrs.Services
{
	[ServiceContract(Namespace = "https://cqrs.co.nz/Domain/1001/")]
	public interface IEventService<TAuthenticationToken>
	{
		[OperationContract]
		IServiceResponseWithResultData<IEnumerable<EventData>> GetEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest);
	}
}