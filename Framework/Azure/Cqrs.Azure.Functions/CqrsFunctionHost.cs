#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Linq;

using Cqrs.Azure.ConfigurationManager;
using Cqrs.Configuration;
using Cqrs.Hosts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#if NET472
#else
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Azure.Functions
{
	/// <summary>
	/// Execute command and event handlers in an Azure WebJob
	/// </summary>
	public abstract class CqrsFunctionHost<TAuthenticationToken> : TelemetryCoreHost<TAuthenticationToken>
	{
		IHost host { get; set; }

		/// <summary>
		/// Indicates if the <see cref="SetExecutionPath"/> method has been called.
		/// </summary>
		protected static bool HasSetExecutionPath { private set; get; }

		/// <summary>
		/// The <see cref="CoreHost"/> that gets executed by the Azure WebJob.
		/// </summary>
		protected static CoreHost<TAuthenticationToken> CoreHost { get; set; }

		/// <summary>
		/// Let's get going by calling this first.
		/// </summary>
		public virtual void Go()
		{
			CoreHost = this;
			StartHost();
			Logger.LogInfo("Application Stopped.");
		}

		/// <summary>
		/// Prepare the host before registering handlers and starting the host.
		/// </summary>
		protected override void Prepare()
		{
			base.Prepare();
			PrepareHost();
		}

		/// <summary>
		/// Prepares the <see cref="IConfigurationManager"/>
		/// </summary>
		protected static void PrepareConfigurationManager()
		{
			IConfigurationManager configurationManager;
#if NET472
			configurationManager = new CloudConfigurationManager();
			DependencyResolver.ConfigurationManager = _configurationManager;
#else
			string localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
			string azureRoot = Environment.GetEnvironmentVariable("HOME");
			azureRoot = string.IsNullOrWhiteSpace(azureRoot)
				? string.Empty
				: $"{azureRoot}/site/wwwroot";

			string actualRoot = localRoot ?? azureRoot ?? Environment.CurrentDirectory;

			// C# ConfigurationBuilder example for Azure Functions v2 runtime
			IConfigurationRoot config = new ConfigurationBuilder()
				.SetBasePath(actualRoot)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			configurationManager = new CloudConfigurationManager(config);
			SetExecutionPath(config);
#endif
		}

		/// <remarks>
		/// Please set the following connection strings in app.config for this WebJob to run:
		/// AzureWebJobsDashboard and AzureWebJobsStorage
		/// Better yet, add them to your Azure portal so they can be changed at runtime without re-deploying or re-compiling.
		/// </remarks>
		protected static void StartHost()
		{
			// We use console state as, even though a webjob runs in an azure website, it's technically loaded via something call the 'WindowsScriptHost', which is not web and IIS based so it's threading model is very different and more console based.
			// This actually does all the work... Just sit back and relax... but also stay in memory and don't shutdown.
			CoreHost.Run();
		}

		/// <summary>
		/// Prepares the <see cref="IHost"/>.
		/// </summary>
		protected virtual void PrepareHost()
		{
#if NET472
			FunctionsDebugger.Enable();
#endif

			host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults(builder => { }, options =>
				{
					options.EnableUserCodeException = true;
				})
				.ConfigureServices(services =>
				{
					ConfigureApplicationInsights(services);
					ConfigureHostServices(services);
				})
				.Build();
		}

		/// <summary>
		/// Configures the isolated process application to emit logs directly Application Insights.
		/// As per https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#application-insights
		/// </summary>
		protected virtual void ConfigureApplicationInsights(IServiceCollection services)
		{
			services.AddApplicationInsightsTelemetryWorkerService();
			services.ConfigureFunctionsApplicationInsights();
			services.Configure<LoggerFilterOptions>(options =>
			{
				// The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
				// Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
				LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

				if (toRemove != null)
					options.Rules.Remove(toRemove);
			});
		}

		/// <summary>
		/// Configures the services of the <see cref="IHost"/>
		/// As per https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#application-insights
		/// </summary>
		protected virtual void ConfigureHostServices(IServiceCollection services)
		{
		}

		/// <summary>
		/// Start the host post preparing and registering handlers.
		/// Then calls <see cref="HostingAbstractionsHostExtensions.Run"/> on the <see cref="IHost"/>.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			host.Run();
		}
		/// <summary>
		/// Sets the execution path
		/// </summary>
		public static void SetExecutionPath
		(
#if NET6_0
			Microsoft.Extensions.Configuration.IConfigurationRoot config
#endif
		)
		{
#if NET6_0
			SetConfigurationManager(config);
#endif

			string localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
			string azureRoot = Environment.GetEnvironmentVariable("HOME");
			azureRoot = string.IsNullOrWhiteSpace(azureRoot)
				? string.Empty
				: $"{azureRoot}/site/wwwroot";

			string actualRoot = localRoot ?? azureRoot ?? Environment.CurrentDirectory;

			Configuration.ConfigurationExtensions.GetExecutionPath = () => Path.Combine(actualRoot);
			HasSetExecutionPath = true;
		}
	}

	class ExampleCqrsFunctionHost
		: CqrsFunctionHost<Guid>
	{
		/// <summary>
		/// Entry point for the issolated application.
		/// </summary>
		public static void Main(string[] args)
		{
			PrepareConfigurationManager();
			new ExampleCqrsFunctionHost().Go();
		}

		protected override void ConfigureDefaultDependencyResolver()
		{
			throw new NotImplementedException();
		}
	}
}