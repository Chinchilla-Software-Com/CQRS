namespace Chat.RestAPI
{
	using System;
	using System.Web;
	using System.Web.Http;
	using Cqrs.Authentication;
	using Cqrs.Configuration;
	using Cqrs.Events;
	using Cqrs.WebApi;
	using Cqrs.WebApi.SignalR.Hubs;

	public class WebApiApplication : CqrsHttpApplication<Guid>
	{
		protected override void ConfigureDefaultDependencyResolver()
		{
			DependencyResolver = Cqrs.Configuration.DependencyResolver.Current;
		}

		protected override void ConfigureMvc()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
		}

		#region Overrides of CqrsHttpApplication<Guid>

		protected override void RegisterSignalR(BusRegistrar registrar)
		{
			base.RegisterSignalR(registrar);
			// Start the event bus receiving messages once everything is in place.
			DependencyResolver.Resolve<IEventReceiver<Guid>>().Start();
			// Inform the NotificationHub that the token is the user identifier and that it can be cast directly.
			var notificationHub = DependencyResolver.Resolve<INotificationHub>() as NotificationHub;
			if (notificationHub != null)
				notificationHub.ConvertUserTokenToUserRsn = token => new Guid(token);
		}

		#endregion

		protected override void Application_BeginRequest(object sender, EventArgs e)
		{
			base.Application_BeginRequest(sender, e);
			string xToken = Request.Headers["X-Token"];
			if (string.IsNullOrWhiteSpace(xToken))
			{
				HttpCookie authCookie = Request.Cookies["X-Token"];
				if (authCookie != null)
					xToken = authCookie.Value;
			}
			Guid token;
			if (Guid.TryParse(xToken, out token))
			{
				// Pass the authentication token to the helper to allow automated authentication handling
				DependencyResolver.Resolve<IAuthenticationTokenHelper<Guid>>().SetAuthenticationToken(token);
			}
		}
	}
}