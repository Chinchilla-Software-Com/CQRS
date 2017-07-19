using System.Web.Routing;

namespace Chat.RestAPI
{
	using Cqrs.Configuration;
	using Cqrs.WebApi;
	using Cqrs.WebApi.SignalR.Hubs;
	using System;
	using System.Web.Http;

	public class WebApiApplication : CqrsHttpApplicationWithSignalR<Guid>
	{
		#region Overrides of CqrsHttpApplication<Guid>

		protected override void ConfigureMvcOrWebApi()
		{
			GlobalConfiguration.Configure
			(
				config =>
				{
					MvcConfig.Register();
					WebApiConfig.Register(config);
				}
			);
		}

		protected override void RegisterSignalR(BusRegistrar registrar)
		{
			base.RegisterSignalR(registrar);

			// Inform the NotificationHub that the token is the user identifier and that it can be cast directly.
			var notificationHub = DependencyResolver.Current.Resolve<INotificationHub>() as NotificationHub;
			if (notificationHub != null)
				notificationHub.ConvertUserTokenToUserRsn = token => new Guid(token);
		}

		#endregion
	}
}