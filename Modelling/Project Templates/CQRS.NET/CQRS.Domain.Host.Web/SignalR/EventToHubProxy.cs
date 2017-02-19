using Cqrs.Authentication;

namespace $safeprojectname$.SignalR
{
	public partial class EventToHubProxy : Cqrs.WebApi.Events.Handlers.EventToHubProxy<SingleSignOnToken>
	{
		public EventToHubProxy(cdmdotnet.Logging.ILogger logger, Cqrs.WebApi.SignalR.Hubs.NotificationHub notificationHub, IAuthenticationTokenHelper<SingleSignOnToken> authenticationTokenHelper)
			: base(logger, notificationHub, authenticationTokenHelper)
		{
		}
	}
}