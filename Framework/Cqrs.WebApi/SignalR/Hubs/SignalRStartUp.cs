using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;

[assembly: OwinStartup(typeof(Cqrs.WebApi.SignalR.Hubs.SignalRStartUp))]
namespace Cqrs.WebApi.SignalR.Hubs
{
	public class SignalRStartUp
	{
		public virtual void Configuration(IAppBuilder app)
		{
			// Any connection or hub wire up and configuration should go here
			app.MapSignalR();

			JsonSerializer serializer = JsonSerializer.Create(GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
		}
	}
}