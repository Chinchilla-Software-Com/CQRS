#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;
using Cqrs.Events;
using Cqrs.WebApi.SignalR.Hubs;

namespace Cqrs.WebApi.Events.Handlers
{
	public abstract class EventToHubProxy<TSingleSignOnToken>
		where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		protected EventToHubProxy(NotificationHub notificationHub, IAuthenticationTokenHelper<TSingleSignOnToken> authenticationTokenHelper)
		{
			NotificationHub = notificationHub;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		protected NotificationHub NotificationHub { get; private set; }

		protected IAuthenticationTokenHelper<TSingleSignOnToken> AuthenticationTokenHelper { get; private set; }

		protected virtual void HandleGenericEvent(IEvent<TSingleSignOnToken> message)
		{
			NotificationHub.SendUserEvent(message, (AuthenticationTokenHelper.GetAuthenticationToken().Token ?? string.Empty).Replace(".", string.Empty));
		}
	}
}