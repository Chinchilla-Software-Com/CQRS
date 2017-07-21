[assembly: WebActivatorEx.PreApplicationStartMethod(typeof($safeprojectname$.Code.ChatConfiguration), "ConfigureNinject", Order = 40)]
[assembly: WebActivatorEx.PreApplicationStartMethod(typeof($safeprojectname$.Code.ChatConfiguration), "ConfigureWcf", Order = 60)]

namespace $safeprojectname$.Code
{
	using Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration;
	using Cqrs.Ninject.Configuration;
	// Don't remove this one
	using StartUp = Cqrs.Ninject.Azure.Wcf.Configuration.SimplifiedNinjectWcf;
	using MicroServices.Configuration;
	using System;

	public class ChatConfiguration
	{
		public static void ConfigureNinject()
		{
			NinjectDependencyResolver.ModulesToLoad.Add(new QueriesModule());
			NinjectDependencyResolver.ModulesToLoad.Add(new AzureCommandBusPublisherModule<Guid>());
			NinjectDependencyResolver.ModulesToLoad.Add(new SimplifiedSqlModule<Guid>());
		}

		public static void ConfigureWcf()
		{
		}
	}
}