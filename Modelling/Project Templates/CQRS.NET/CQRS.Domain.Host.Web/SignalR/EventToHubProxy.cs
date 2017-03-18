using cdmdotnet.Logging;
using Cqrs.Authentication;

namespace $safeprojectname$.SignalR
{
	public partial class EventToHubProxy : Cqrs.WebApi.Events.Handlers.EventToHubProxy<SingleSignOnToken>
	{
		public EventToHubProxy(ILogger logger, Cqrs.WebApi.SignalR.Hubs.NotificationHub notificationHub, IAuthenticationTokenHelper<SingleSignOnToken> authenticationTokenHelper)
			: base(logger, notificationHub, authenticationTokenHelper)
		{
		}
	}
}