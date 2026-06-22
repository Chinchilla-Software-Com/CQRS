#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;
using Cqrs.Services;

namespace Cqrs.WebApi
{
	/// <summary>
	/// A <see cref="CqrsApiController"/> that includes an implementation of the <see cref="IEventService{TAuthenticationToken}.GetEventData"/> method
	/// </summary>
	public abstract class CqrsEventApiController<TSingleSignOnToken>
		: CqrsApiController
		, IEventService<TSingleSignOnToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="CqrsEventApiController{TSingleSignOnToken}"/>.
		/// </summary>
		protected CqrsEventApiController(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TSingleSignOnToken> authenticationTokenHelper, IEventStore<TSingleSignOnToken> eventStore, IConfigurationManager configurationManager)
			: base(logger, correlationIdHelper, configurationManager)
		{
			AuthenticationTokenHelper = authenticationTokenHelper;
			EventStore = eventStore;
		}

		/// <summary>
		/// Gets or set the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAuthenticationTokenHelper<TSingleSignOnToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="IEventStore{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IEventStore<TSingleSignOnToken> EventStore { get; private set; }


		#region Implementation of IEventService<SingleSignOnToken>

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}">events</see>
		/// raised with the same <see cref="IMessage.CorrelationId"/>.
		/// </summary>
		/// <param name="serviceRequest">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}">events</see> to find.</param>
		/// <returns>A collection of <see cref="EventData">event data</see></returns>
		IServiceResponseWithResultData<IEnumerable<EventData>> IEventService<TSingleSignOnToken>.GetEventData(IServiceRequestWithData<TSingleSignOnToken, Guid> serviceRequest)
		{
			return GetEventData(serviceRequest);
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}">events</see>
		/// raised with the same <see cref="IMessage.CorrelationId"/>.
		/// </summary>
		/// <param name="serviceRequest">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}">events</see> to find.</param>
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

		#endregion

		/// <summary>
		/// Executed before calling the <see cref="IEventStore{TAuthenticationToken}.Get(System.Type,System.Guid,bool,int)"/> method on <see cref="EventStore"/>
		/// in <see cref="GetEventData"/>.
		/// </summary>
		/// <param name="serviceRequest">The original <see cref="IServiceRequestWithData{TAuthenticationToken,Guid}"/>.</param>
		protected virtual void OnGetEventData(IServiceRequestWithData<TSingleSignOnToken, Guid> serviceRequest) { }

		/// <summary>
		/// Executed after calling the <see cref="IEventStore{TAuthenticationToken}.Get(System.Type,System.Guid,bool,int)"/> method on <see cref="EventStore"/>
		/// in <see cref="GetEventData"/>.
		/// </summary>
		/// <param name="serviceRequest">The original <see cref="IServiceRequestWithData{TAuthenticationToken,Guid}"/>.</param>
		/// <param name="results">The collection of <see cref="IEvent{TAuthenticationToken}">events</see> from the <see cref="EventStore"/>.</param>
		protected virtual IEnumerable<EventData> OnGotEventData(IServiceRequestWithData<TSingleSignOnToken, Guid> serviceRequest, IEnumerable<EventData> results)
		{
			return results;
		}
	}
}
