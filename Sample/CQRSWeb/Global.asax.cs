using System.Web.Mvc;
using System.Web.Routing;
using CQRSCode.WriteModel.Domain;
using CQRSCode.WriteModel.Handlers;
using Cqrs.Commands;
using Cqrs.Authentication;
using Cqrs.Configuration;
using IDependencyResolver = Cqrs.Configuration.IDependencyResolver;

namespace CQRSWeb
{	
	public class MvcApplication : System.Web.HttpApplication
	{

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);

		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
			RegisterHandlers((IDependencyResolver)DependencyResolver.Current);
		}

		private void RegisterHandlers(IDependencyResolver serviceLocator)
		{
			var registrar = new BusRegistrar(serviceLocator);
			registrar.Register(typeof(InventoryCommandHandlers), typeof(DtoCommandHandler<ISingleSignOnToken, UserDto>));
		}
	}
}