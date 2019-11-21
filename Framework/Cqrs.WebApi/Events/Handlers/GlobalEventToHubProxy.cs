#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.WebApi.SignalR.Hubs;

namespace Cqrs.WebApi.Events.Handlers
{
	/// <summary>
	/// Proxies ALL <see cref="IEvent{TAuthenticationToken}">events</see> received from the event bus to the <see cref="INotificationHub"/>.
	/// This gets registered as a global <see cref="IEventHandler"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class GlobalEventToHubProxy<TAuthenticationToken>
		: EventToHubProxy<TAuthenticationToken>
		, IEventHandler<TAuthenticationToken, IEvent<TAuthenticationToken>>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="GlobalEventToHubProxy{TAuthenticationToken}"/>.
		/// </summary>
		public GlobalEventToHubProxy(ILogger logger, INotificationHub notificationHub, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			: base(logger, notificationHub, authenticationTokenHelper)
		{
		}

		#region Implementation of IMessageHandler<in IEvent<TAuthenticationToken>>

		/// <summary>
		/// Calls <see cref="EventToHubProxy{TAuthenticationToken}.HandleGenericEvent"/>.
		/// </summary>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to proxy.</param>
		public void Handle(IEvent<TAuthenticationToken> @event)
		{
			HandleGenericEvent(@event);
		}

		#endregion
	}
}