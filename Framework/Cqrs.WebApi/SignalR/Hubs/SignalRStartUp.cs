#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Web.Http;
using Cqrs.Configuration;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Owin;

[assembly: OwinStartup(typeof(Cqrs.WebApi.SignalR.Hubs.SignalRStartUp))]
namespace Cqrs.WebApi.SignalR.Hubs
{
	public class SignalRStartUp
	{
		public virtual void Configuration(IAppBuilder app, IConfigurationManager configurationManager)
		{
			string url;
			if (!configurationManager.TryGetSetting("Cqrs.WebApi.SignalR.EndpointPath", out url) || string.IsNullOrWhiteSpace(url))
				url = "/signalr";
			// Branch the pipeline here for requests that start with "/signalr"
			app.Map(url, map =>
			{
				// Setup the CORS middleware to run before SignalR.
				// By default this will allow all origins. You can 
				// configure the set of origins and/or http verbs by
				// providing a cors options with a different policy.
				map.UseCors(CorsOptions.AllowAll);
				var hubConfiguration = new HubConfiguration
				{
					// You can enable JSONP by un-commenting line below.
					// JSONP requests are insecure but some older browsers (and some
					// versions of IE) require JSONP to work cross domain
					// EnableJSONP = true
					EnableDetailedErrors = true,
//					Resolver = new SignalRResolver()
				};
				// Run the SignalR pipeline. We're not using MapSignalR
				// since this branch already runs under the "/signalr"
				// path.
				map.RunSignalR(hubConfiguration);
			});

			JsonSerializer serializer = JsonSerializer.Create(GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings);
			GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
		}
	}
}