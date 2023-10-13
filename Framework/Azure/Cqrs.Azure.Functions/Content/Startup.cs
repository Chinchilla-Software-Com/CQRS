using System;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(FunctionStartup))]
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