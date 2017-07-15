#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.WebApi.SignalR.Hubs;

namespace Cqrs.WebApi.Events.Handlers
{
	public class GlobalEventToHubProxy<TAuthenticationToken>
		: EventToHubProxy<TAuthenticationToken>
		, IEventHandler<TAuthenticationToken, IEvent<TAuthenticationToken>>
	{
		public GlobalEventToHubProxy(ILogger logger, INotificationHub notificationHub, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			: base(logger, notificationHub, authenticationTokenHelper)
		{
		}

		#region Implementation of IMessageHandler<in IEvent<TAuthenticationToken>>

		public void Handle(IEvent<TAuthenticationToken> @event)
		{
			HandleGenericEvent(@event);
		}

		#endregion
	}
}