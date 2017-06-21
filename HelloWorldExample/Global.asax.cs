namespace HelloWorld
{
	using System;
	using System.Web;
	using System.Web.Http;
	using System.Web.Mvc;
	using System.Web.Optimization;
	using System.Web.Routing;
	using Cqrs.Authentication;
	using Cqrs.Ninject.Configuration;
	using Cqrs.WebApi;
	using Code;

	public class MvcApplication : CqrsHttpApplication<string, EventToHubProxy>
	{
		protected override void ConfigureDefaultDependencyResolver()
		{
			DependencyResolver = NinjectDependencyResolver.Current;
		}

		protected override void ConfigureMvc()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected override void Application_BeginRequest(object sender, EventArgs e)
		{
			base.Application_BeginRequest(sender, e);
			HttpCookie authCookie = Request.Cookies[".AspNet.ApplicationCookie"];
			if (authCookie != null)
			{
				// Copy encrypted auth token to X-Token for SignalR
				Response.SetCookie(new HttpCookie("X-Token", authCookie.Value));
				// Pass the auth token to the helper to allow automated authentication handling
				DependencyResolver.Resolve<IAuthenticationTokenHelper<string>>().SetAuthenticationToken(authCookie.Value);
			}
		}
	}
}