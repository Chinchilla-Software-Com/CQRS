#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

[assembly: FunctionsStartup(typeof(Cqrs.Scheduler.Trigger.Startup))]
namespace Cqrs.Scheduler.Trigger
{
	public class Startup
		: FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{

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
		}
	}
}