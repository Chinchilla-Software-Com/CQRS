[assembly: Microsoft.Owin.OwinStartup(typeof(Chat.RestAPI.Code.Startup))]

namespace Chat.RestAPI.Code
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