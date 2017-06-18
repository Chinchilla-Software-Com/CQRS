using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HelloWorldExample.Startup))]
namespace HelloWorldExample
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
