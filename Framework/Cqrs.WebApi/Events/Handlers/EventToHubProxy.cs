#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.WebApi.SignalR.Hubs;

namespace Cqrs.WebApi.Events.Handlers
{
	public abstract class EventToHubProxy<TAuthenticationToken>
	{
		protected EventToHubProxy(ILogger logger, INotificationHub notificationHub, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			Logger = logger;
			NotificationHub = notificationHub;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		protected ILogger Logger { get; private set; }

		protected INotificationHub NotificationHub { get; private set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected virtual void HandleGenericEvent(IEvent<TAuthenticationToken> message)
		{
			Type eventType = message.GetType();
			var notifyCallerEventAttribute = Attribute.GetCustomAttribute(eventType, typeof(NotifyCallerEventAttribute)) as NotifyCallerEventAttribute;
			var notifyEveryoneEventAttribute = Attribute.GetCustomAttribute(eventType, typeof(NotifyEveryoneEventAttribute)) as NotifyEveryoneEventAttribute;
			var notifyEveryoneExceptCallerEventAttribute = Attribute.GetCustomAttribute(eventType, typeof(NotifyEveryoneExceptCallerEventAttribute)) as NotifyEveryoneExceptCallerEventAttribute;

			string userToken = ((object)AuthenticationTokenHelper.GetAuthenticationToken() ?? string.Empty).ToString().Replace(".", string.Empty);

			if (notifyCallerEventAttribute != null)
			{
				NotificationHub.SendUserEvent(message, userToken);
			}
			if (notifyEveryoneEventAttribute != null)
			{
				NotificationHub.SendAllUsersEvent(message);
			}
			if (notifyEveryoneExceptCallerEventAttribute != null)
			{
				NotificationHub.SendExceptThisUserEvent(message, userToken);
			}
		}
	}
}