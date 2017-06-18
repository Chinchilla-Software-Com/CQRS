using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Cqrs.Authentication;
using Cqrs.Ninject.Configuration;
using Cqrs.WebApi;
using HelloWorldExample.Controllers.SignalR;

namespace HelloWorldExample
{
	public class MvcApplication : CqrsHttpApplication<string, EventToHubProxy>
	{
		#region Overrides of CqrsHttpApplication<string,EventToHubProxy>

		protected override void ConfigureDefaultDependencyResolver()
		{
			DependencyResolver = NinjectDependencyResolver.Current;
		}

		protected override void ConfigureMvc()
		{
			AreaRegistration.RegisterAllAreas();
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

		#endregion
	}
}
