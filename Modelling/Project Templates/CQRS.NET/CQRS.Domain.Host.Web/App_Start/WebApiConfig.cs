using System.Collections.Generic;
using System.Web.Http;
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
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

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