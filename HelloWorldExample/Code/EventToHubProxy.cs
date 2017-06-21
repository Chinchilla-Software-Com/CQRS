namespace HelloWorld.Code
{
	using cdmdotnet.Logging;
	using Cqrs.Authentication;
	using Cqrs.Events;
	using Cqrs.WebApi.SignalR.Hubs;
	using Events;

	public class EventToHubProxy
		: Cqrs.WebApi.Events.Handlers.EventToHubProxy<string>
		, IEventHandler<string, HelloSaidEvent>
	{
		public EventToHubProxy(ILogger logger, INotificationHub notificationHub, IAuthenticationTokenHelper<string> authenticationTokenHelper)
			: base(logger, notificationHub, authenticationTokenHelper)
		{
		}

		#region Implementation of IMessageHandler<in HelloSaidEvent>

		public virtual void Handle(HelloSaidEvent message)
		{
			HandleGenericEvent(message);
		}

		#endregion
	}
}