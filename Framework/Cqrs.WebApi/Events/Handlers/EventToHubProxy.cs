#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.WebApi.SignalR.Hubs;

namespace Cqrs.WebApi.Events.Handlers
{
	public abstract class EventToHubProxy<TSingleSignOnToken>
		where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		protected EventToHubProxy(INotificationHub notificationHub, IAuthenticationTokenHelper<TSingleSignOnToken> authenticationTokenHelper)
		{
			NotificationHub = notificationHub;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		protected INotificationHub NotificationHub { get; private set; }

		protected IAuthenticationTokenHelper<TSingleSignOnToken> AuthenticationTokenHelper { get; private set; }

		protected virtual void HandleGenericEvent(IEvent<TSingleSignOnToken> message)
		{
			Type eventType = message.GetType();
			var notifyCallerEventAttribute = Attribute.GetCustomAttribute(eventType, typeof(NotifyCallerEventAttribute)) as NotifyCallerEventAttribute;
			var notifyEveryoneEventAttribute = Attribute.GetCustomAttribute(eventType, typeof(NotifyEveryoneEventAttribute)) as NotifyEveryoneEventAttribute;
			var notifyEveryoneExceptCallerEventAttribute = Attribute.GetCustomAttribute(eventType, typeof(NotifyEveryoneExceptCallerEventAttribute)) as NotifyEveryoneExceptCallerEventAttribute;

			string userToken = (AuthenticationTokenHelper.GetAuthenticationToken().Token ?? string.Empty).Replace(".", string.Empty);

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