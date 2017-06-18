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

[assembly: OwinStartup("Cqrs.WebApi.SignalR.Hubs.SignalRStartUp", typeof(Cqrs.WebApi.SignalR.Hubs.SignalRStartUp))]
namespace Cqrs.WebApi.SignalR.Hubs
{
	public class SignalRStartUp
	{
		public IConfigurationManager ConfigurationManager { get; set; }

		public SignalRStartUp()
			:this (new ConfigurationManager())
		{
		}

		public SignalRStartUp(IConfigurationManager configurationManager)
		{
			ConfigurationManager = configurationManager;
		}

		public virtual void Configuration(IAppBuilder app)
		{
			string url;
			if (!ConfigurationManager.TryGetSetting("Cqrs.WebApi.SignalR.EndpointPath", out url) || string.IsNullOrWhiteSpace(url))
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

			JsonSerializer serializer = JsonSerializer.Create
			(
				new JsonSerializerSettings
				{
					Formatting = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Formatting,
					MissingMemberHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.MissingMemberHandling,
					DateParseHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateParseHandling,
					DateTimeZoneHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling,
					Converters = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters,
					ContractResolver = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver,
					DateFormatHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling,
					DefaultValueHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DefaultValueHandling,
					FloatFormatHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.FloatFormatHandling,
					NullValueHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling,
					PreserveReferencesHandling = PreserveReferencesHandling.None,
					ReferenceLoopHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling,
					StringEscapeHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.StringEscapeHandling,
					TypeNameHandling = TypeNameHandling.None
				}
			);
			GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
		}
	}
}