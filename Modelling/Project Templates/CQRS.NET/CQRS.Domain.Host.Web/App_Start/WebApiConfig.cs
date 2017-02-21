using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Ninject.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Cqrs.WebApi.Formatters.FormMultipart;
using Cqrs.WebApi.Formatters.FormMultipart.Infrastructure;

namespace $safeprojectname$
{
	public class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			var configurationManager = new CloudConfigurationManager();
			var cors = new EnableCorsAttribute
			(
				configurationManager.GetSetting("Cqrs.WebApi.Cors.Origins"),
				configurationManager.GetSetting("Cqrs.WebApi.Cors.Headers"),
				configurationManager.GetSetting("Cqrs.WebApi.Cors.Methods"),
				configurationManager.GetSetting("Cqrs.WebApi.Cors.ExposedHeaders")
			);
			config.EnableCors(cors);
			config.MapHttpAttributeRoutes();

			/*
			config.Routes.MapHttpRoute(
				name: "DefaultApiWithAction",
				routeTemplate: "{controller}/{action}/{id}",
				defaults: new { action = "Index", id = RouteParameter.Optional }
			);

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
			*/

			GlobalConfiguration.Configuration.DependencyResolver = NinjectDependencyResolver.Current.Resolve<System.Web.Http.Dependencies.IDependencyResolver>();
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DateParseHandling = DateParseHandling.DateTimeOffset,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				Converters = new List<JsonConverter> { new StringEnumConverter() },
			};

			GlobalConfiguration.Configuration.Formatters.Add(new FormMultipartEncodedMediaTypeFormatter(new MultipartFormatterSettings()));
		}
	}
}