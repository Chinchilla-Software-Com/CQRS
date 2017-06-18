using System.Web.Http;

namespace Cqrs.WebApi.Configuration
{
	public class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();

			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings;
		}
	}
}
