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
	/// <summary>
	/// Proxies <see cref="IEvent{TSingleSignOnToken}">events</see> from the event bus to the <see cref="INotificationHub"/>.
	/// This requires one or more <see cref="IEventHandler"/> implementations in order to be triggered.
	/// </summary>
	/// <typeparam name="TSingleSignOnToken">The <see cref="ISingleSignOnToken"/> of the authentication token.</typeparam>
	public abstract class SingleSignOnTokenEventToHubProxy<TSingleSignOnToken>
		where TSingleSignOnToken : ISingleSignOnToken, new()
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="SingleSignOnTokenEventToHubProxy{TSingleSignOnToken}"/>.
		/// </summary>
		protected SingleSignOnTokenEventToHubProxy(ILogger logger, INotificationHub notificationHub, IAuthenticationTokenHelper<TSingleSignOnToken> authenticationTokenHelper)
		{
			Logger = logger;
			NotificationHub = notificationHub;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// The <see cref="INotificationHub"/> to proxy <see cref="IEvent{TAuthenticationToken}">events</see> to.
		/// </summary>
		protected INotificationHub NotificationHub { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TSingleSignOnToken}"/>.
		/// </summary>
		protected IAuthenticationTokenHelper<TSingleSignOnToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Get the authentication token of the entity that triggered the request generating the provided <paramref name="message"/>
		/// Extract any proxy information attributes (<see cref="NotifyCallerEventAttribute"/>, <see cref="NotifyEveryoneEventAttribute"/> and <see cref="NotifyEveryoneExceptCallerEventAttribute"/>)
		/// then proxy the provided <paramref name="message"/> to <see cref="NotificationHub"/> if an attribute is present.
		/// </summary>
		/// <param name="message">The <see cref="IEvent{TSingleSignOnToken}"/> to proxy.</param>
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