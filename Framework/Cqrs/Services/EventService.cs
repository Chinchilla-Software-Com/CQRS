#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.ServiceModel;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Services
{
	/// <summary>
	/// A WCF <see cref="ServiceContractAttribute">ServiceContract</see> that provides read-only access to <see cref="IEventStore{TAuthenticationToken}"/> <see cref="IEvent{TAuthenticationToken}">events</see>
	/// raised with the same <see cref="IMessage.CorrelationId"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public abstract class EventService<TAuthenticationToken> : IEventService<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="EventService{TAuthenticationToken}"/>.
		/// </summary>
		protected EventService(IEventStore<TAuthenticationToken> eventStore, ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			EventStore = eventStore;
			Logger = logger;
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		/// <summary>
		/// Gets or sets the <see cref="IEventStore{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IEventStore<TAuthenticationToken> EventStore { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}">events</see>
		/// raised with the same <see cref="IMessage.CorrelationId"/>.
		/// </summary>
		/// <param name="serviceRequest">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}">events</see> to find.</param>
		public virtual IServiceResponseWithResultData<IEnumerable<EventData>> GetEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest)
		{
			AuthenticationTokenHelper.SetAuthenticationToken(serviceRequest.AuthenticationToken);
			CorrelationIdHelper.SetCorrelationId(serviceRequest.CorrelationId);

			OnGetEventData(serviceRequest);
			IEnumerable<EventData> results = EventStore.Get(serviceRequest.Data);
			results = OnGotEventData(serviceRequest, results);

			return CompleteResponse
			(
				new ServiceResponseWithResultData<IEnumerable<EventData>>
				{
					State = ServiceResponseStateType.Succeeded,
					ResultData = results,
				}
			);
		}

		/// <summary>
		/// Executed before calling the <see cref="IEventStore{TAuthenticationToken}.Get(System.Type,System.Guid,bool,int)"/> method on <see cref="EventStore"/>
		/// in <see cref="GetEventData"/>.
		/// </summary>
		/// <param name="serviceRequest">The original <see cref="IServiceRequestWithData{TAuthenticationToken,Guid}"/>.</param>
		protected virtual void OnGetEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest) { }

		/// <summary>
		/// Executed after calling the <see cref="IEventStore{TAuthenticationToken}.Get(System.Type,System.Guid,bool,int)"/> method on <see cref="EventStore"/>
		/// in <see cref="GetEventData"/>.
		/// </summary>
		/// <param name="serviceRequest">The original <see cref="IServiceRequestWithData{TAuthenticationToken,Guid}"/>.</param>
		/// <param name="results">The collection of <see cref="IEvent{TAuthenticationToken}">events</see> from the <see cref="EventStore"/>.</param>
		protected virtual IEnumerable<EventData> OnGotEventData(IServiceRequestWithData<TAuthenticationToken, Guid> serviceRequest, IEnumerable<EventData> results)
		{
			return results;
		}

		/// <summary>
		/// Sets the <see cref="IServiceResponse.CorrelationId"/> on the provided <paramref name="serviceResponse"/>.
		/// </summary>
		protected virtual TServiceResponse CompleteResponse<TServiceResponse>(TServiceResponse serviceResponse)
			where TServiceResponse : IServiceResponse
		{
			serviceResponse.CorrelationId = CorrelationIdHelper.GetCorrelationId();
			return serviceResponse;
		}
	}
}