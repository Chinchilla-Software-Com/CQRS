#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Cqrs.Authentication;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Azure.Functions.Isolated.Configuration;
using Cqrs.DependencyInjection;
using Cqrs.DependencyInjection.Modules;
using Cqrs.Hosts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Builder;

#if NET48
#else
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Azure.Functions.Isolated
{
	/// <summary>
	/// Execute command and event handlers in an Azure WebJob
	/// </summary>
	public class CqrsIsolatedFunctionHost<TAuthenticationToken, TAuthenticationTokenHelper, TIsolatedFunctionHostModule>
		: TelemetryCoreHost<TAuthenticationToken>
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
		where TIsolatedFunctionHostModule : IsolatedFunctionHostModule, new()
	{
		/// <summary>
		/// The <see cref="IHostBuilder"/> the <see cref="Host"/> is made from.
		/// </summary>
		protected IHostBuilder HostBuilder { get; set; }

		/// <summary>
		/// The <see cref="IHostApplicationBuilder"/> the <see cref="Host"/> is made from.
		/// </summary>
		protected FunctionsApplicationBuilder HostApplicationBuilder { get; set; }

		/// <summary>
		/// The <see cref="IHost"/> that drives this function.
		/// </summary>
		protected IHost Host { get; set; }

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
		/// Instantiate a new instance of <see cref="CqrsIsolatedFunctionHost{TAuthenticationToken, TAuthenticationTokenHelper, TIsolatedFunctionHostModule}"/>
		/// </summary>
		public CqrsIsolatedFunctionHost()
		{
			HandlerTypes = GetCommandOrEventHandlerTypes();
		}

		/// <summary>
		/// Prepare the host before registering handlers and starting the host.
		/// </summary>
		protected override void Prepare()
		{
			PrepareHost();
			base.Prepare();
		}

		/// <summary>
		/// Add JUST ONE command and/or event handler here from each assembly you want automatically scanned.
		/// </summary>
		protected virtual Type[] GetCommandOrEventHandlerTypes()
		{
			return new Type[] { };
		}

		/// <summary>
		/// Prepares the <see cref="Cqrs.Configuration.IConfigurationManager"/>
		/// </summary>
		protected static void PrepareConfigurationManager(ConfigurationBuilder configBuilder = null)
		{
			Cqrs.Configuration.IConfigurationManager configurationManager;
#if NET48
			configurationManager = new CloudConfigurationManager();
			DependencyResolver.ConfigurationManager = _configurationManager;
#else
			string localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
			string azureRoot = Environment.GetEnvironmentVariable("HOME");
			azureRoot = string.IsNullOrWhiteSpace(azureRoot)
				? null
				: $"{azureRoot}/site/wwwroot";

			string actualRoot = localRoot ?? azureRoot ?? Environment.CurrentDirectory;

			// C# ConfigurationBuilder example for Azure Functions v2 runtime
			IConfigurationBuilder _configBuilder = (configBuilder ?? new ConfigurationBuilder())
				.SetBasePath(actualRoot)
				.AddCommandLine(Environment.GetCommandLineArgs())
				.AddJsonFile("cqrs.settings.json", optional: true, reloadOnChange: false)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
				.AddEnvironmentVariables();

			// copied from Microsoft.Extensions.Hosting.WorkerHostBuilderExtensions
			var switchMappings = new Dictionary<string, string>
			{
				{ "--functions-uri", "Functions:Worker:HostEndpoint" },
				{ "--functions-request-id", "Functions:Worker:RequestId" },
				{ "--functions-worker-id", "Functions:Worker:WorkerId" },
				{ "--functions-grpc-max-message-length", "Functions:Worker:GrpcMaxMessageLength" },
			};
			_configBuilder.AddCommandLine(Environment.GetCommandLineArgs(), switchMappings);

			IConfigurationRoot config = _configBuilder.Build();
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
		/// Creates the <see cref="IHostBuilder"/>.
		/// </summary>
		protected virtual void CreateHostBuilder()
		{
			HostBuilder = new HostBuilder();   
		}

		/// <summary>
		/// Creates the <see cref="IHostApplicationBuilder"/>.
		/// </summary>
		protected virtual void CreateApplicationHostBuilder()
		{
			var _hostApplicationBuilder = FunctionsApplication.CreateBuilder(Environment.GetCommandLineArgs());
			HostApplicationBuilder = _hostApplicationBuilder;

		}

		/// <summary>
		/// Prepares the <see cref="IHost"/>.
		/// </summary>
		protected virtual void PrepareHost()
		{
#if NET48
			FunctionsDebugger.Enable();
#endif
			CreateHostBuilder();
			CreateApplicationHostBuilder();

			Func<IConfigurationBuilder, IConfigurationBuilder> cfgBuilder = (configBuilder) =>
			{
				string localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
				string azureRoot = Environment.GetEnvironmentVariable("HOME");
				azureRoot = string.IsNullOrWhiteSpace(azureRoot)
					? null
					: $"{azureRoot}/site/wwwroot";

				string actualRoot = localRoot ?? azureRoot ?? Environment.CurrentDirectory;

				(configBuilder ?? (configBuilder = new ConfigurationBuilder()))
					.SetBasePath(actualRoot)
					.AddCommandLine(Environment.GetCommandLineArgs())
					.AddJsonFile("cqrs.settings.json", optional: true, reloadOnChange: false)
					.AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
					.AddEnvironmentVariables();

				return configBuilder;
			};
				HostBuilder
					.ConfigureFunctionsWorkerDefaults(builder => {
				}, options =>
				{
				})
				.ConfigureAppConfiguration(configBuilder =>
				{
#if NET48
#else
					cfgBuilder(configBuilder);
#endif
				})
				.ConfigureServices(services =>
				{
					ConfigureApplicationInsights(services);
					ConfigureHostServices(services);
				});
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

			Host.Run();
		}
		/// <summary>
		/// Sets the execution path
		/// </summary>
		public static void SetExecutionPath
		(
#if NET6_0_OR_GREATER
			Microsoft.Extensions.Configuration.IConfigurationRoot config
#endif
		)
		{
#if NET6_0_OR_GREATER
			SetConfigurationManager(config);
#endif

			string localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
			string azureRoot = Environment.GetEnvironmentVariable("HOME");
			azureRoot = string.IsNullOrWhiteSpace(azureRoot)
				? null
				: $"{azureRoot}/site/wwwroot";

			string actualRoot = localRoot ?? azureRoot ?? Environment.CurrentDirectory;

			Cqrs.Configuration.ConfigurationExtensions.GetExecutionPath = () => Path.Combine(actualRoot, "bin");
			HasSetExecutionPath = true;
		}

		/// <summary>
		/// Configure the <see cref="DependencyResolver"/>.
		/// </summary>
		protected override void ConfigureDefaultDependencyResolver()
		{
			Action<IServiceCollection> configureDependencyResolver = (services) =>
			{
				foreach (Module supplementaryModule in GetSupplementaryModules(services))
					DependencyResolver.ModulesToLoad.Add(supplementaryModule);
				DependencyResolver.Start(services, prepareProvidedKernel: true);
			};

			/*
			var _host = HostBuilder
				.ConfigureServices(services => {
					configureDependencyResolver(services);
				})
				.Build();
			*/

			configureDependencyResolver(HostApplicationBuilder.Services);
			Host = HostApplicationBuilder.Build();

			((DependencyResolver)Cqrs.Configuration.DependencyResolver.Current).SetKernel(Host.Services);
		}

		/// <summary>
		/// A collection of <see cref="Module"/> that are required to be loaded
		/// </summary>
		protected virtual IEnumerable<Module> GetSupplementaryModules(IServiceCollection services)
		{
			var results = new List<Module>
			{
				new TIsolatedFunctionHostModule(),
#if NET6_0_OR_GREATER
				new CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper>(new CloudConfigurationManager(Cqrs.Configuration.ConfigurationManager.BaseConfiguration))
#else
				new CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper>(new CloudConfigurationManager())
#endif
			};

//			results.AddRange(GetCommandBusModules(services));
//			results.AddRange(GetEventBusModules(services));
			results.AddRange(GetEventStoreModules(services));

			return results;
		}

		/*
		/// <summary>
		/// A collection of <see cref="Module"/> that configure the Azure Servicebus as a command bus as both
		/// <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IEnumerable<Module> GetCommandBusModules(IServiceCollection services)
		{
			var list = new List<Module> { new AzureCommandBusPublisherModule<TAuthenticationToken>() };
			bool setting;

			if (!ConfigurationManager.TryGetSetting("Cqrs.Hosts.EnableCommandReceiving", out setting))
				setting = true;
			if (setting)
				list.Add(new AzureCommandBusReceiverModule<TAuthenticationToken>());

			return list;
		}

		/// <summary>
		/// A collection of <see cref="Module"/> that configure the Azure Servicebus as a event bus as both
		/// <see cref="IEventPublisher{TAuthenticationToken}"/> and <see cref="IEventReceiver{TAuthenticationToken}"/>
		/// If the app setting Cqrs.Host.EnableEventReceiving is "false" then no modules will be returned.
		/// </summary>
		protected virtual IEnumerable<Module> GetEventBusModules(IServiceCollection services)
		{
			var list = new List<Module> { new AzureEventBusPublisherModule<TAuthenticationToken>() };
			bool setting;

			if (!ConfigurationManager.TryGetSetting("Cqrs.Hosts.EnableEventReceiving", out setting))
				setting = true;
			if (setting)
				list.Add(new AzureEventBusReceiverModule<TAuthenticationToken>());

			return list;
		}
		*/

		/// <summary>
		/// A collection of <see cref="Module"/> that configure SQL server as the <see cref="Events.IEventStore{TAuthenticationToken}"/>
		/// </summary>
		protected virtual IEnumerable<Module> GetEventStoreModules(IServiceCollection services)
		{
			return new List<Module>
			{
				new SimplifiedSqlModule<TAuthenticationToken>()
			};
		}
	}

	class ExampleCqrsIsolatedFunctionHost
		: CqrsIsolatedFunctionHost<Guid, DefaultAuthenticationTokenHelper, IsolatedFunctionHostModule>
	{
		/// <summary>
		/// Entry point for the issolated application.
		/// </summary>
		public static void Main(string[] args)
		{
			PrepareConfigurationManager();
			new ExampleCqrsIsolatedFunctionHost().Go();
		}
	}
}