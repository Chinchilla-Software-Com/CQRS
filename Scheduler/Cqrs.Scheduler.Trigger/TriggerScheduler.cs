#if NET472
#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;

using Chinchilla.Logging;
using Chinchilla.Logging.Azure.Storage;
using Chinchilla.Logging.Configuration;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Ninject.Configuration;
using Cqrs.Scheduler.Commands;

using Microsoft.Azure.WebJobs;
#if NET472
#else
using Microsoft.Extensions.Configuration;
#endif

using ExecutionContext = System.Threading.ExecutionContext;

namespace Cqrs.TriggerScheduler
{
	public static class TriggerScheduler
	{
		[FunctionName(nameof(TriggerScheduler))]
#if DEBUG
		// Run every one minute in debug mode
		public static async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, Microsoft.Extensions.Logging.ILogger _logger, ExecutionContext context)
#else
		// Run every 15 minutes in release mode
		public static async Task RunAsync([TimerTrigger("0 1/15 * * * *")]TimerInfo myTimer, Microsoft.Extensions.Logging.ILogger _logger, ExecutionContext context)
#endif
		{
			if (CommandPublisher == null)
				PreapreOnce(context);

			CorrelationIdHelper.SetCorrelationId(Guid.NewGuid());
			CommandPublisher.Publish(new PublishTimeZonesCommand());
			Console.WriteLine($"Published.");
			Logger.LogInfo($"Published.");

			await Task.CompletedTask;
		}

		static ICommandPublisher<Guid> CommandPublisher { get; set; }

		static ICorrelationIdHelper CorrelationIdHelper { get; set; }

		static ILogger Logger { get; set; }

		static void PreapreOnce(ExecutionContext context)
		{
			Console.WriteLine("IMPORTANT: Make sure you have read the ReadMeFirst.txt file");
			IConfigurationManager configurationManager;
#if NET472
			configurationManager = new CloudConfigurationManager();
#else
			// C# ConfigurationBuilder example for Azure Functions v2 runtime
			IConfigurationRoot config = new ConfigurationBuilder()
				.SetBasePath(context.FunctionAppDirectory)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			configurationManager = new CloudConfigurationManager(config);
			CqrsFunction.SetExecutionPath(context, config);
#endif

			new CqrsFunction().Run();

			// Configure Table storage for logging... see settings file to set connection string
			((NinjectDependencyResolver)DependencyResolver.Current).Kernel.Unbind(typeof(ILogger));
			var logger = new TableStorageLogger(DependencyResolver.Current.Resolve<ILoggerSettings>(), DependencyResolver.Current.Resolve<ICorrelationIdHelper>(), DependencyResolver.Current.Resolve<ITelemetryHelper>());
			((NinjectDependencyResolver)DependencyResolver.Current).Kernel.Bind<ILogger>().ToConstant(logger);

			CommandPublisher = DependencyResolver.Current.Resolve<ICommandPublisher<Guid>>();
			CorrelationIdHelper = DependencyResolver.Current.Resolve<ICorrelationIdHelper>();
			Logger = DependencyResolver.Current.Resolve<ILogger>();
		}
	}
}
#endif