using System;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Cqrs.Scheduler.SampleReport.FunctionStartup))]
namespace Cqrs.Scheduler.SampleReport
{
	public class FunctionStartup
		: FunctionsStartup
	{
		private IConfigurationBuilder _configurationBuilder;

		private string _applicationRootPath;

		public override void Configure(IFunctionsHostBuilder builder)
		{
			Program.Main(_applicationRootPath, _configurationBuilder.Build(), builder.Services);
		}

		public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
		{
			FunctionsHostBuilderContext context = builder.GetContext();
#if NET48
#else
			string localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
			string azureRoot = Environment.GetEnvironmentVariable("HOME");
			azureRoot = string.IsNullOrWhiteSpace(azureRoot)
				? string.Empty
				: $"{azureRoot}/site/wwwroot";

			string actualRoot = localRoot ?? azureRoot ?? context.ApplicationRootPath ?? Environment.CurrentDirectory;

			// C# ConfigurationBuilder example for Azure Functions v2 runtime
			builder.ConfigurationBuilder
				.SetBasePath(context.ApplicationRootPath)
				.AddCommandLine(Environment.GetCommandLineArgs())
				.AddJsonFile("cqrs.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();
#endif

			_configurationBuilder = builder.ConfigurationBuilder;
			_applicationRootPath = context.ApplicationRootPath;
		}
	}
}