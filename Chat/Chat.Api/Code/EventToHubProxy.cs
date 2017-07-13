namespace Chat.Api.Code
{
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
	using Cqrs.WebApi.SignalR.Hubs;

	public class EventToHubProxy
		: Cqrs.WebApi.Events.Handlers.EventToHubProxy<string>
	{
		public EventToHubProxy(ILogger logger, INotificationHub notificationHub, IAuthenticationTokenHelper<string> authenticationTokenHelper)
			: base(logger, notificationHub, authenticationTokenHelper)
		{
		}
	}
}