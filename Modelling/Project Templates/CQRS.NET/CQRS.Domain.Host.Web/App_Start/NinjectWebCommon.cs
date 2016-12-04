using System.Web.Mvc;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof($safeprojectname$.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof($safeprojectname$.NinjectWebCommon), "Stop")]

namespace $safeprojectname$
{
	using Microsoft.Web.Infrastructure.DynamicModuleHelper;

	using Ninject;
	using Ninject.Web.Common;

	public static class NinjectWebCommon
	{
		private static readonly Bootstrapper bootstrapper = new Bootstrapper();

		/// <summary>
		/// Starts the application
		/// </summary>
		public static void Start()
		{
			DynamicModuleUtility.RegisterModule(typeof (OnePerRequestHttpModule));
			DynamicModuleUtility.RegisterModule(typeof (NinjectHttpModule));
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
			var host = new WebHost();
			host.Configure();
			host.Start();

			try
			{
				RegisterServices(host.Kernel);
				return host.Kernel;
			}
			catch
			{
				host.Kernel.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Load your modules or register your services here!
		/// </summary>
		/// <param name="kernel">The kernel.</param>
		private static void RegisterServices(IKernel kernel)
		{
			// Tell ASP.NET MVC 3 to use our Ninject DI Container 
			DependencyResolver.SetResolver(new Ninject.Web.Mvc.NinjectDependencyResolver(kernel));
		}
	}
}