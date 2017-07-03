#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Events;

namespace Cqrs.Services
{
	public abstract class EventService<TAuthenticationToken> : IEventService<TAuthenticationToken>
	{
		protected EventService(IEventStore<TAuthenticationToken> eventStore, ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			EventStore = eventStore;
			Logger = logger;
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		protected virtual IEventStore<TAuthenticationToken> EventStore { get; private set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		public virtual IServiceResponseWithResultData<IEnumerable<EventData>> GetEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest)
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

		protected virtual void OnGetEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest) { }

		protected virtual IEnumerable<EventData> OnGotEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest, IEnumerable<EventData> results)
		{
			return results;
		}
	}
}