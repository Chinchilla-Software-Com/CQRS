namespace Chat.Api
{
	using System.Web.Http;
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
			GlobalConfiguration.Configure(WebApiConfig.Register);
		}
	}
}