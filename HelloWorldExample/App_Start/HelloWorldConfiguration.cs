[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(HelloWorld.HelloWorldConfiguration), "ConfigureNinject", Order = 40)]
[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(HelloWorld.HelloWorldConfiguration), "ConfigureMvc", Order = 60)]

namespace HelloWorld
{
	using System.Web.Mvc;
	using Cqrs.Ninject.Configuration;
	using Cqrs.Ninject.InProcess.CommandBus.Configuration;
	using Cqrs.Ninject.InProcess.EventBus.Configuration;

	public static class HelloWorldConfiguration
	{
		public static void ConfigureNinject()
		{
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessCommandBusModule<string>());
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<string>());
		}

		public static void ConfigureMvc()
		{
			// Tell ASP.NET MVC 3 to use our Ninject DI Container 
			DependencyResolver.SetResolver(new Ninject.Web.Mvc.NinjectDependencyResolver(((NinjectDependencyResolver)NinjectDependencyResolver.Current).Kernel));
		}
	}
}
