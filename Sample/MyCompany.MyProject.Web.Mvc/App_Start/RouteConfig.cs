#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Web.Mvc;
using System.Web.Routing;

namespace MyCompany.MyProject.Web.Mvc
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{rsn}",
				defaults: new { controller = "Home", action = "Index", rsn = UrlParameter.Optional }
			);

			routes.MapRoute(
				name: "RestDefault",
				url: "{controller}/{action}/{singleSignOnToken}/{rsn}",
				defaults: new { controller = "Home", action = "Index", singleSignOnToken = UrlParameter.Optional, rsn = UrlParameter.Optional }
			);
		}
	}
}