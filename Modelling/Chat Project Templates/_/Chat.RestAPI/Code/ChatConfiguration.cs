[assembly: WebActivatorEx.PreApplicationStartMethod(typeof($safeprojectname$.Code.ChatConfiguration), "ConfigureNinject", Order = 40)]
[assembly: WebActivatorEx.PreApplicationStartMethod(typeof($safeprojectname$.Code.ChatConfiguration), "ConfigureWebApi", Order = 60)]

namespace $safeprojectname$.Code
{
	using Cqrs.Configuration;
	using Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration;
	using Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration;
	using Cqrs.Ninject.Configuration;
	// Don't remove this one
	using StartUp = Cqrs.Ninject.WebApi.Configuration.SimplifiedNinjectWebApi;
	using MicroServices.Configuration;
	using System;
	using System.Web.Http;

	public class ChatConfiguration
	{
		public static void ConfigureNinject()
		{
			NinjectDependencyResolver.ModulesToLoad.Add(new QueriesModule());
			NinjectDependencyResolver.ModulesToLoad.Add(new AzureCommandBusPublisherModule<Guid>());
			NinjectDependencyResolver.ModulesToLoad.Add(new AzureEventBusReceiverModule<Guid>());
			NinjectDependencyResolver.ModulesToLoad.Add(new ApiModule());
		}

		public static void ConfigureWebApi()
		{
			// Tell ASP.NET WebAPI to use our Ninject DI Container 
			GlobalConfiguration.Configuration.DependencyResolver = new Ninject.Web.WebApi.NinjectDependencyResolver(((NinjectDependencyResolver)DependencyResolver.Current).Kernel);
		}
	}
}