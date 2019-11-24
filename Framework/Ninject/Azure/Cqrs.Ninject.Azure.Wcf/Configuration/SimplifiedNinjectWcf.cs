#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Azure.ConfigurationManager;
using Ninject;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#endif
#if NET472
using Cqrs.Ninject.Configuration;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Cqrs.Ninject.Azure.Wcf.Configuration.SimplifiedNinjectWcf), "Start", Order = 50)]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Cqrs.Ninject.Azure.Wcf.Configuration.SimplifiedNinjectWcf), "Stop", Order = 50)]
#endif

namespace Cqrs.Ninject.Azure.Wcf.Configuration
{
#if NET472
	/// <summary>
	/// A <see cref="WebActivatorEx.PreApplicationStartMethodAttribute"/> that calls <see cref="Start"/>
	/// and <see cref="WebActivatorEx.ApplicationShutdownMethodAttribute"/> that calls <see cref="Stop"/>
	/// configuring Simplified SQL by wiring up <see cref="SimplifiedSqlModule{TAuthenticationToken}"/>.
	/// </summary>
	public static class SimplifiedNinjectWcf
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
			return new WcfStartUp(new CloudConfigurationManager()).CreateKernel();
		}
	}
#endif
#if NETSTANDARD2_0
	/// <summary>
	/// The Startup class that configures Simplified SQL by wiring up an eventual replacement for SimplifiedSqlModule
	/// </summary>
	public class SimplifiedNinjectWcf
	{
		private IKernel Kernel { get; set; }

		/// <summary>
		/// The <see cref="IConfiguration"/> that can be use to get configuration settings
		/// </summary>
		protected IConfiguration Configuration { get; private set; }

		/// <summary>
		/// Instantiate a new instance of a <see cref="SimplifiedNinjectWcf"/>
		/// </summary>
		public SimplifiedNinjectWcf(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		/// <summary>
		/// Creates the kernel that will manage your application.
		/// </summary>
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			Kernel = new WcfStartUp(new CloudConfigurationManager(Configuration)).CreateKernel();
		}
	}
#endif
}