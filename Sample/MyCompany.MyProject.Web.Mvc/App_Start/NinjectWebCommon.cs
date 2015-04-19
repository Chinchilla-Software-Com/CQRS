#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Web;
using System.Web.Mvc;
using Cqrs.Authentication;
using Cqrs.Ninject.Configuration;
using Cqrs.Ninject.InProcess.CommandBus.Configuration;
using Cqrs.Ninject.InProcess.EventBus.Configuration;
using Cqrs.Ninject.InProcess.EventStore.Configuration;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using MyCompany.MyProject.Domain.Configuration;
using Ninject;
using Ninject.Web.Common;
using NinjectDependencyResolver = Ninject.Web.Mvc.NinjectDependencyResolver;

namespace MyCompany.MyProject.Web.Mvc
{
	[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(MyCompany.MyProject.Web.Mvc.NinjectWebCommon), "Start")]
	[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(MyCompany.MyProject.Web.Mvc.NinjectWebCommon), "Stop")]

	public static class NinjectWebCommon 
	{
		private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

		/// <summary>
		/// Starts the application
		/// </summary>
		public static void Start() 
		{
			DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
			DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
			Bootstrapper.Initialize(CreateKernel);
		}
		
		/// <summary>
		/// Stops the application.
		/// </summary>
		public static void Stop()
		{
			Bootstrapper.ShutDown();
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
		internal static void RegisterServices(IKernel kernel)
		{
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Clear();

			// Core Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<ISingleSignOnToken, SingleSignOnTokenValueHelper>());
			// Event Store Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventStoreModule<ISingleSignOnToken>());
			// Command Bus Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new InProcessCommandBusModule<ISingleSignOnToken>());
			// Event Bus Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<ISingleSignOnToken>());
			// Domain Core Module
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.ModulesToLoad.Add(new DomainCoreModule());
			Cqrs.Ninject.Configuration.NinjectDependencyResolver.Start(kernel, true);

			// Tell ASP.NET MVC 3 to use our Ninject DI Container 
			DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
		}
	}
}