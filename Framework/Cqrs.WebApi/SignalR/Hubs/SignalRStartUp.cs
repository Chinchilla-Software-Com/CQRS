#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Configuration;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Owin;

[assembly: OwinStartup("Cqrs.WebApi.SignalR.Hubs.SignalRStartUp", typeof(Cqrs.WebApi.SignalR.Hubs.SignalRStartUp))]
namespace Cqrs.WebApi.SignalR.Hubs
{
	/// <summary>
	/// A Start-up class for SignalR. wired in using <see cref="OwinStartupAttribute">OwinStartup</see>.
	/// </summary>
	public class SignalRStartUp
	{
		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		public IConfigurationManager ConfigurationManager { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="SignalRStartUp"/> creating a new instance of <see cref="ConfigurationManager"/>.
		/// </summary>
		public SignalRStartUp()
			:this (new ConfigurationManager())
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="SignalRStartUp"/> with the provided <paramref name="configurationManager"/>.
		/// </summary>
		public SignalRStartUp(IConfigurationManager configurationManager)
		{
			ConfigurationManager = configurationManager;
		}

		/// <summary>
		/// Configure SignalR to run.
		/// </summary>
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

				// Instruct DI to resolve the Hub
				GlobalHost.DependencyResolver.Register
				(
					typeof(INotificationHub),
					() => DependencyResolver.Current.Resolve(typeof(INotificationHub))
				);
				GlobalHost.DependencyResolver.Register
				(
					typeof(NotificationHub),
					() => DependencyResolver.Current.Resolve(typeof(INotificationHub))
				);
			});

			JsonSerializer serializer = JsonSerializer.Create
			(
				new JsonSerializerSettings
				{
					ContractResolver = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ContractResolver,
					StringEscapeHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.StringEscapeHandling,
					PreserveReferencesHandling = PreserveReferencesHandling.None,
					MissingMemberHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.MissingMemberHandling,
					Binder = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Binder,
					CheckAdditionalContent = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.CheckAdditionalContent,
					ConstructorHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ConstructorHandling,
					Context = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Context,
					Converters = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Converters,
					Culture = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Culture,
					DateFormatHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DateFormatHandling,
					DateFormatString = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DateFormatString,
					DateParseHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DateParseHandling,
					DateTimeZoneHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DateTimeZoneHandling,
					DefaultValueHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.DefaultValueHandling,
					Error = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Error,
					FloatFormatHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.FloatFormatHandling,
					FloatParseHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.FloatParseHandling,
					Formatting = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.Formatting,
					MaxDepth = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.MaxDepth,
					MetadataPropertyHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.MetadataPropertyHandling,
					NullValueHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.NullValueHandling,
					ObjectCreationHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ObjectCreationHandling,
					ReferenceLoopHandling = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ReferenceLoopHandling,
					ReferenceResolver = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.ReferenceResolver,
					TraceWriter = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.TraceWriter,
					TypeNameAssemblyFormat = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings.TypeNameAssemblyFormat,
					TypeNameHandling = TypeNameHandling.None
				}
			);
			GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
		}
	}
}