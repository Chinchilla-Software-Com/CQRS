using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.Services;

namespace Cqrs.WebApi
{
	/// <summary>
	/// A <see cref="CqrsApiController"/> that includes an implementation of the <see cref="IEventService{TAuthenticationToken}.GetEventData"/> method
	/// </summary>
	public abstract class CqrsEventApiController<TSingleSignOnToken>
		: CqrsApiController
		where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		protected CqrsEventApiController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TSingleSignOnToken> authenticationTokenHelper, IEventStore<TSingleSignOnToken> eventStore)
			: base(logger, correlationIdHelper)
		{
			AuthenticationTokenHelper = authenticationTokenHelper;
			EventStore = eventStore;
		}

		protected IAuthenticationTokenHelper<TSingleSignOnToken> AuthenticationTokenHelper { get; private set; }

		protected virtual IEventStore<TSingleSignOnToken> EventStore { get; private set; }

		/// <summary>
		/// Query for all the events that match the provided CorrelationId.
		/// </summary>
		/// <param name="serviceRequest">A <see cref="IServiceRequestWithData{TAuthenticationToken,TData}">service-request</see> that contains the CorrelationId.</param>
		/// <returns>A collection of <see cref="EventData">event data</see></returns>
		protected virtual IServiceResponseWithResultData<IEnumerable<EventData>> GetEventData(IServiceRequestWithData<TSingleSignOnToken, Guid> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

			OnGetEventData(serviceRequest);
			IEnumerable<EventData> results = EventStore.Get(serviceRequest.Data);
			results = OnGotEventData(serviceRequest, results);

			return new ServiceResponseWithResultData<IEnumerable<EventData>>
			{
				State = ServiceResponseStateType.Succeeded,
				ResultData = results,
				CorrelationId = CorrelationIdHelper.GetCorrelationId()
			};
		}

		protected virtual void OnGetEventData(IServiceRequestWithData<TSingleSignOnToken, Guid> serviceRequest) { }

		protected virtual IEnumerable<EventData> OnGotEventData(IServiceRequestWithData<TSingleSignOnToken, Guid> serviceRequest, IEnumerable<EventData> results)
		{
			return results;
		}
	}
}
