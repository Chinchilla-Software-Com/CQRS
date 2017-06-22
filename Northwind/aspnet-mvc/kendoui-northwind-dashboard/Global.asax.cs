
namespace KendoUI.Northwind.Dashboard
{
	using System;
	using System.Web.Http;
	using System.Web.Mvc;
	using System.Web.Routing;
	using cdmdotnet.Logging;
	using Code;
	using Cqrs.Configuration;
	using Cqrs.Ninject.Configuration;
	using Cqrs.WebApi;
	using Cqrs.WebApi.Events.Handlers;

	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : CqrsHttpApplication<string, EventToHubProxy<string>>
	{
		protected override void ConfigureDefaultDependencyResolver()
		{
			DependencyResolver = NinjectDependencyResolver.Current;
		}

		protected override void ConfigureMvc()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}

		protected override void Application_BeginRequest(object sender, EventArgs e)
		{
			try
			{
				ICorrelationIdHelper correlationIdHelper = NinjectDependencyResolver.Current.Resolve<ICorrelationIdHelper>();
				correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			}
			catch (NullReferenceException) { }
		}

		protected override BusRegistrar RegisterCommandAndEventHandlers()
		{
			BusRegistrar registrar = base.RegisterCommandAndEventHandlers();
			registrar.Register(typeof(Order));
			return registrar;
		}

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "RegionalSalesStatus", id = UrlParameter.Optional }
			);
		}
	}
}