using System;
using System.Web.Http;

namespace Cqrs.WebApi.Configuration
{
	public class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			try
			{
				config.MapHttpAttributeRoutes();
			}
			catch (ArgumentException exception)
			{
				if (exception.ParamName != "name")
					throw;
			}

			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = Cqrs.Events.DefaultJsonSerializerSettings.DefaultSettings;
		}
	}
}
