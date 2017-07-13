[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Chat.Api.Code.ChatConfiguration), "ConfigureNinject", Order = 40)]
[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Chat.Api.Code.ChatConfiguration), "ConfigureMvc", Order = 60)]

namespace Chat.Api.Code
{
	using Ninject.Web.WebApi;
	using MicroServices.Configuration;
	using System.Web.Http;

	public class ChatConfiguration
	{
		public static void ConfigureNinject()
		{
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new QueriesModule());
			//			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new InProcessCommandBusModule<string>());
			//			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<string>());
		}

		public static void ConfigureMvc()
		{
			// Tell ASP.NET WebAPI to use our Ninject DI Container 
			GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(((Cqrs.Ninject.Configuration.NinjectDependencyResolver)Cqrs.Ninject.Configuration.NinjectDependencyResolver.Current).Kernel);
		}
	}
}