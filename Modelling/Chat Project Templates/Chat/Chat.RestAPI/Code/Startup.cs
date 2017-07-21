[assembly: Microsoft.Owin.OwinStartup(typeof($safeprojectname$.Code.Startup))]

namespace $safeprojectname$.Code
{
	using Owin;

	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			new Cqrs.WebApi.SignalR.Hubs.SignalRStartUp().Configuration(app);
		}
	}
}