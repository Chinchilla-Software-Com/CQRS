#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Cqrs.WebApi.SignalR.Hubs
{
	/// <summary>
	/// Sends <see cref="IEvent{TAuthenticationToken}">events</see> to different groups of users via a SignalR <see cref="Hub"/>.
	/// </summary>
	public class NotificationHub
		: Hub
		, INotificationHub
		, ISingleSignOnTokenNotificationHub
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="NotificationHub"/>.
		/// </summary>
		public NotificationHub(ILogger logger, ICorrelationIdHelper correlationIdHelper)
		{
			Logger = logger;
			CorrelationIdHelper = correlationIdHelper;
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="NotificationHub"/>.
		/// </summary>
		public NotificationHub()
		{
		}

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		public ILogger Logger { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		public ICorrelationIdHelper CorrelationIdHelper { get; set; }

		/// <summary>
		/// The <see cref="Func{String, Guid}"/> that can convert a <see cref="string"/> based authentication token into the <see cref="Guid"/> based user identifier.
		/// </summary>
		public Func<string, Guid> ConvertUserTokenToUserRsn { get; set; }

		#region Overrides of HubBase

		/// <summary>
		/// When the connection connects to this hub instance we register the connection so we can respond back to it.
		/// </summary>
		public override Task OnConnected()
		{
			return Join();
		}

		/// <summary>
		/// When the connection reconnects to this hub instance we register the connection so we can respond back to it.
		/// </summary>
		public override Task OnReconnected()
		{
			return Join();
		}

		#endregion

		/// <summary>
		/// Gets the authentication token for the user from the incoming hub request looking at first the 
		/// <see cref="HubCallerContext.RequestCookies"/> and then the <see cref="HubCallerContext.QueryString"/>.
		/// The authentication token should have a name matching the value of "Cqrs.Web.AuthenticationTokenName" from <see cref="IConfigurationManager.GetSetting"/>.
		/// </summary>
		protected virtual string UserToken()
		{
			string userRsn;
			Cookie cookie;

			string authenticationTokenName = DependencyResolver.Current.Resolve<IConfigurationManager>().GetSetting("Cqrs.Web.AuthenticationTokenName") ?? "X-Token";

			if (Context.RequestCookies.TryGetValue(authenticationTokenName, out cookie))
				userRsn = cookie.Value;
			else
				userRsn = Context.QueryString[authenticationTokenName];

			return userRsn
				.Replace(".", string.Empty)
				.Replace("-", string.Empty);
		}

		/// <summary>
		/// Join the authenticated user to their relevant <see cref="IHubContext.Groups"/>.
		/// </summary>
		protected virtual Task Join()
		{
			string userToken = UserToken();
			string connectionId = Context.ConnectionId;
			return Task.Factory.StartNewSafely(() =>
			{
				Task work = Groups.Add(connectionId, string.Format("User-{0}", userToken));
				work.ConfigureAwait(false);
				work.Wait();

				CurrentHub
					.Clients
					.Group(string.Format("User-{0}", userToken))
					.registered("User: " + userToken);

				if (ConvertUserTokenToUserRsn != null)
				{
					try
					{
						Guid userRsn = ConvertUserTokenToUserRsn(userToken);
						work = Groups.Add(connectionId, string.Format("UserRsn-{0}", userRsn));
						work.ConfigureAwait(false);
						work.Wait();

						CurrentHub
							.Clients
							.Group(string.Format("UserRsn-{0}", userRsn))
							.registered("UserRsn: " + userRsn);

					}
					catch (Exception exception)
					{
						Logger.LogWarning(string.Format("Registering user token '{0}' to a user RSN and into the SignalR group failed.", userToken), exception: exception, metaData: GetAdditionalDataForLogging(userToken));
					}
				}
			});
		}

		/// <summary>
		/// Gets the current <see cref="IHubContext"/>.
		/// </summary>
		protected virtual IHubContext CurrentHub
		{
			get
			{
				return GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
			}
		}

		/// <summary>
		/// Send out an event to specific user RSNs
		/// </summary>
		void INotificationHub.SendUsersEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, params Guid[] userRsnCollection)
		{
			IList<Guid> optimisedUserRsnCollection = (userRsnCollection ?? Enumerable.Empty<Guid>()).ToList();

			Logger.LogDebug(string.Format("Sending a message on the hub for user RSNs [{0}].", string.Join(", ", optimisedUserRsnCollection)));

			try
			{
				var tokenSource = new CancellationTokenSource();
				Task.Factory.StartNewSafely
				(
					() =>
					{
						foreach (Guid userRsn in optimisedUserRsnCollection)
						{
							var metaData = GetAdditionalDataForLogging(userRsn);
							try
							{
								Clients
									.Group(string.Format("UserRsn-{0}", userRsn))
									.notifyEvent(new { Type = eventData.GetType().FullName, Data = eventData, CorrelationId = CorrelationIdHelper.GetCorrelationId() });
							}
							catch (TimeoutException exception)
							{
								Logger.LogWarning("Sending a message on the hub timed-out.", exception: exception, metaData: metaData);
							}
							catch (Exception exception)
							{
								Logger.LogError("Sending a message on the hub resulted in an error.", exception: exception, metaData: metaData);
							}
						}
					}, tokenSource.Token
				);

				tokenSource.CancelAfter(15 * 1000);
			}
			catch (Exception exception)
			{
				foreach (Guid userRsn in optimisedUserRsnCollection)
					Logger.LogError("Queueing a message on the hub resulted in an error.", exception: exception, metaData: GetAdditionalDataForLogging(userRsn));
			}
		}

		/// <summary>
		/// Send out an event to specific user token
		/// </summary>
		void INotificationHub.SendUserEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, string userToken)
		{
			Logger.LogDebug(string.Format("Sending a message on the hub for user [{0}].", userToken));

			try
			{
				var tokenSource = new CancellationTokenSource();
				Task.Factory.StartNewSafely
				(
					() =>
					{
						IDictionary<string, object> metaData = GetAdditionalDataForLogging(userToken);

						try
						{
							CurrentHub
								.Clients
								.Group(string.Format("User-{0}", userToken))
								.notifyEvent(new { Type = eventData.GetType().FullName, Data = eventData, CorrelationId = CorrelationIdHelper.GetCorrelationId() });
						}
						catch (TimeoutException exception)
						{
							Logger.LogWarning("Sending a message on the hub timed-out.", exception: exception, metaData: metaData);
						}
						catch (Exception exception)
						{
							Logger.LogError("Sending a message on the hub resulted in an error.", exception: exception, metaData: metaData);
						}
					}, tokenSource.Token
				);

				tokenSource.CancelAfter(15 * 1000);
			}
			catch (Exception exception)
			{
				Logger.LogError("Queueing a message on the hub resulted in an error.", exception: exception, metaData: GetAdditionalDataForLogging(userToken));
			}
		}

		/// <summary>
		/// Send out an event to all users
		/// </summary>
		void INotificationHub.SendAllUsersEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData)
		{
			Logger.LogDebug("Sending a message on the hub to all users.");

			try
			{
				var tokenSource = new CancellationTokenSource();
				Task.Factory.StartNewSafely
				(
					() =>
					{
						try
						{
							CurrentHub
								.Clients
								.All
								.notifyEvent(new { Type = eventData.GetType().FullName, Data = eventData, CorrelationId = CorrelationIdHelper.GetCorrelationId() });
						}
						catch (TimeoutException exception)
						{
							Logger.LogWarning("Sending a message on the hub to all users timed-out.", exception: exception);
						}
						catch (Exception exception)
						{
							Logger.LogError("Sending a message on the hub to all users resulted in an error.", exception: exception);
						}
					}, tokenSource.Token
				);

				tokenSource.CancelAfter(15 * 1000);
			}
			catch (Exception exception)
			{
				Logger.LogError("Queueing a message on the hub to all users resulted in an error.", exception: exception);
			}
		}

		/// <summary>
		/// Send out an event to all users except the specific user token
		/// </summary>
		void INotificationHub.SendExceptThisUserEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, string userToken)
		{
			Logger.LogDebug(string.Format("Sending a message on the hub for all users except user [{0}].", userToken));

			return;

			/*
			try
			{
				var tokenSource = new CancellationTokenSource();
				Task.Factory.StartNewSafely
				(
					() =>
					{
						var metaData = GetAdditionalDataForLogging(userToken);

						try
						{
							CurrentHub
								.Clients
								.Group(string.Format("User-{0}", userToken))
								.notifyEvent(new { Type = eventData.GetType().FullName, Data = eventData, CorrelationId = CorrelationIdHelper.GetCorrelationId() });
						}
						catch (TimeoutException exception)
						{
							Logger.LogWarning(string.Format("Sending a message on the hub for all users except user [{0}] timed-out.", userToken), exception: exception, metaData: metaData);
						}
						catch (Exception exception)
						{
							Logger.LogError(string.Format("Sending a message on the hub for all users except user [{0}] resulted in an error.", userToken), exception: exception, metaData: metaData);
						}
					}, tokenSource.Token
				);

				tokenSource.CancelAfter(15 * 1000);
			}
			catch (Exception exception)
			{
				Logger.LogError("Queueing a message on the hub resulted in an error.", exception: exception, metaData: GetAdditionalDataForLogging(userToken));
			}
			*/
		}

		/// <summary>
		/// Send out an event to specific user token
		/// </summary>
		void ISingleSignOnTokenNotificationHub.SendUserEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, string userToken)
		{
			((INotificationHub) this).SendUserEvent(eventData, userToken);
		}

		/// <summary>
		/// Send out an event to all users
		/// </summary>
		void ISingleSignOnTokenNotificationHub.SendAllUsersEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData)
		{
			((INotificationHub)this).SendAllUsersEvent(eventData);
		}

		/// <summary>
		/// Send out an event to all users except the specific user token
		/// </summary>
		void ISingleSignOnTokenNotificationHub.SendExceptThisUserEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, string userToken)
		{
			((INotificationHub)this).SendExceptThisUserEvent(eventData, userToken);
		}

		/// <summary>
		/// Send out an event to specific user RSNs
		/// </summary>
		void ISingleSignOnTokenNotificationHub.SendUsersEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, params Guid[] userRsnCollection)
		{
			((INotificationHub)this).SendUsersEvent(eventData, userRsnCollection);
		}

		/// <summary>
		/// Create additional data containing the provided <paramref name="userRsn"/>.
		/// </summary>
		protected virtual IDictionary<string, object> GetAdditionalDataForLogging(Guid userRsn)
		{
			return new Dictionary<string, object> { { "UserRsn", userRsn } };
		}

		/// <summary>
		/// Create additional data containing the provided <paramref name="userToken"/>.
		/// </summary>
		protected virtual IDictionary<string, object> GetAdditionalDataForLogging(string userToken)
		{
			return new Dictionary<string, object> { { "UserToken", userToken } };
		}
	}
}