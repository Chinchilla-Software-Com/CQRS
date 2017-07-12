[assembly: Microsoft.Owin.OwinStartup(typeof(Chat.Api.Code.Startup))]

namespace Chat.Api.Code
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