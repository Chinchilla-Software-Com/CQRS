[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(KendoUI.Northwind.Dashboard.NorthwindConfiguration), "ConfigureNinject", Order = 40)]
[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(KendoUI.Northwind.Dashboard.NorthwindConfiguration), "ConfigureMvc", Order = 60)]

namespace KendoUI.Northwind.Dashboard
{
	using System.Web.Mvc;
	using Cqrs.Ninject.Configuration;
	using Cqrs.Ninject.InProcess.CommandBus.Configuration;
	using Cqrs.Ninject.InProcess.EventBus.Configuration;

	public static class NorthwindConfiguration
	{
		public static void ConfigureNinject()
		{
			NinjectDependencyResolver.ModulesToLoad.Add(new NorthwindModule());
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessCommandBusModule<string>());
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<string>());
			NinjectDependencyResolver.ModulesToLoad.Add(new SimplifiedSqlModule<string>());
		}

		public static void ConfigureMvc()
		{
			// Tell ASP.NET MVC 3 to use our Ninject DI Container 
			DependencyResolver.SetResolver(new Ninject.Web.Mvc.NinjectDependencyResolver(((NinjectDependencyResolver)NinjectDependencyResolver.Current).Kernel));
		}
	}
}