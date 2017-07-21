namespace $safeprojectname$
{
	using System.Web.Http;

	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}