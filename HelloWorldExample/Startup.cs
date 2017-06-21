using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HelloWorld.Startup))]

namespace HelloWorld
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			ConfigureAuth(app);
			new Cqrs.WebApi.SignalR.Hubs.SignalRStartUp().Configuration(app);
		}
	}
}