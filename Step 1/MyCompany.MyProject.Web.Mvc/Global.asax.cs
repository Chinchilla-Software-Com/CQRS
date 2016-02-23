using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Cqrs.Authentication;

namespace MyCompany.MyProject.Web.Mvc
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}

		protected void Session_Start(object sender, EventArgs e)
		{
			if (HttpContext.Current.Session["UserToken"] == null)
				HttpContext.Current.Session["UserToken"] = new SingleSignOnTokenFactory().CreateNew();
		}
	}
}