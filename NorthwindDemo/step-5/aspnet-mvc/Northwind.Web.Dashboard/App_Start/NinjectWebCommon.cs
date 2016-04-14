[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Northwind.Web.Dashboard.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Northwind.Web.Dashboard.NinjectWebCommon), "Stop")]

namespace Northwind.Web.Dashboard
{
    using System;
    using System.Web;
	using System.Web.Mvc;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

	using Northwind.Domain.Configuration;
	using Northwind.Configuration;

	using Cqrs.Authentication;
	using Cqrs.Ninject.Configuration;
	using Cqrs.Ninject.InProcess.CommandBus.Configuration;
	using Cqrs.Ninject.InProcess.EventBus.Configuration;
	using NinjectDependencyResolver = Ninject.Web.Mvc.NinjectDependencyResolver;

	public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Clear();

			// Base Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new BaseModule());

			// Core Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<ISingleSignOnToken, SingleSignOnTokenValueHelper>());
			// Event Store Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new SimplifiedSqlModule<ISingleSignOnToken>());
			// Command Bus Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new InProcessCommandBusModule<ISingleSignOnToken>());
			// Event Bus Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<ISingleSignOnToken>());
			// Domain Core Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new DomainNinjectModule());
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.Start(kernel, true);

			// Tell ASP.NET MVC 3 to use our Ninject DI Container 
			DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
		}
    }
}
