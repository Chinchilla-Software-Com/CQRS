namespace Chat.Api
{
	using System.Web.Http;
	using System.Web.Routing;
	using Code;
	using Cqrs.Ninject.Configuration;
	using Cqrs.WebApi;

	public class WebApiApplication : CqrsHttpApplication<string, EventToHubProxy>
	{
		protected override void ConfigureDefaultDependencyResolver()
		{
			DependencyResolver = NinjectDependencyResolver.Current;
		}

		protected override void ConfigureMvc()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}
	}
}