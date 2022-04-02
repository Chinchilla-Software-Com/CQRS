#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

#if NET472
#else
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Ninject.Azure.Wcf.Configuration;
using Microsoft.Extensions.Configuration;
#endif

using Chinchilla.Logging;
using Chinchilla.Logging.Azure.Storage;
using Chinchilla.Logging.Configuration;
using Cqrs.Configuration;
using Cqrs.Ninject.Configuration;
using Cqrs.Scheduler.SampleReport.EventHandlers;

namespace Cqrs.Scheduler.SampleReport
{
	/// <summary>
	/// Starts the WebJob.
	/// </summary>
	public partial class SampleReportWebJob : CqrsWebJobProgram
	{
		protected override Type[] GetCommandOrEventTypes()
		{
			return new[] { typeof(SendAnEmailAtMidnight) };
		}

		/// <remarks>
		/// Please set the following connection strings in app.config for this WebJob to run:
		/// AzureWebJobsDashboard and AzureWebJobsStorage
		/// Better yet, add them to your Azure portal so they can be changed at runtime without re-deploying or re-compiling.
		/// </remarks>
		public static void Main()
		{
#if NET472
#else
			// C# ConfigurationBuilder example for Azure Functions v2 runtime
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			WebHostModule.Configuration = config;
			_configurationManager = new CloudConfigurationManager(config);
#endif

			Console.WriteLine("IMPORTANT: Make sure you have read the ReadMeFirst.txt file");
			new SampleReportWebJob().Go();
		}

		/// <summary>
		/// Rebind the logger to Azure Table Storage
		/// </summary>
		protected override void ConfigureDefaultDependencyResolver()
		{
			base.ConfigureDefaultDependencyResolver();
			((NinjectDependencyResolver)DependencyResolver.Current).Kernel.Unbind(typeof(ILogger));
			var logger = new TableStorageLogger(DependencyResolver.Current.Resolve<ILoggerSettings>(), DependencyResolver.Current.Resolve<ICorrelationIdHelper>(), DependencyResolver.Current.Resolve<ITelemetryHelper>());
			((NinjectDependencyResolver)DependencyResolver.Current).Kernel.Bind<ILogger>().ToConstant(logger);
		}
	}
}