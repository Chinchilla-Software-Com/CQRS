using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Services
{
	public interface IEventService<TAuthenticationToken>
	{
		IServiceResponseWithResultData<IEnumerable<EventData>> GetEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest);
	}
}