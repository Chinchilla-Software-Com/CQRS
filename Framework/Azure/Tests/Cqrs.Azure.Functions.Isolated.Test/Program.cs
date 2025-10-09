using Cqrs.Authentication;
using Cqrs.Azure.Functions.Isolated;
using Cqrs.Azure.Functions.Isolated.Configuration;
using Cqrs.DependencyInjection.Modules;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public partial class Program
	: CqrsIsolatedFunctionHost<Guid, DefaultAuthenticationTokenHelper, IsolatedFunctionHostModule>
{
	/// <summary>
	/// Entry point for the issolated application.
	/// </summary>
	public static void Main(string[] args)
	{
		PrepareConfigurationManager();
		var prg = new Program();
		prg.Go();

		var builder = FunctionsApplication.CreateBuilder(args);

		builder.ConfigureFunctionsWebApplication();

		builder.Services
			.AddApplicationInsightsTelemetryWorkerService()
			.ConfigureFunctionsApplicationInsights();

		builder.Build().Run();

	}

	protected override void CreateApplicationHostBuilder()
	{
		base.CreateApplicationHostBuilder();
		HostApplicationBuilder.ConfigureFunctionsWebApplication();
	}

	/// <summary>
	/// A collection of <see cref="Module"/> that are required to be loaded
	/// </summary>
	protected override IEnumerable<Module> GetSupplementaryModules(IServiceCollection services)
	{
		List<Module> results = base.GetSupplementaryModules(services).ToList();

		results.AddRange(GetCommandBusModules(services));
		results.AddRange(GetEventBusModules(services));
		results.AddRange(GetLoggerModules(services));

		return results;
	}

	/// <summary>
	/// A collection of <see cref="Module"/> that configure the InProcess command bus as both
	/// <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver{TAuthenticationToken}"/>.
	/// </summary>
	protected virtual IEnumerable<Module> GetCommandBusModules(IServiceCollection services)
	{
		var list = new List<Module> { new InProcessCommandBusModule<Guid>() };

		return list;
	}

	/// <summary>
	/// A collection of <see cref="Module"/> that configure the InProcess event bus as both
	/// <see cref="IEventPublisher{TAuthenticationToken}"/> and <see cref="IEventReceiver{TAuthenticationToken}"/>.
	/// </summary>
	protected virtual IEnumerable<Module> GetEventBusModules(IServiceCollection services)
	{
		var list = new List<Module> { new InProcessEventBusModule<Guid>() };

		return list;
	}

	/// <summary>
	/// A collection of <see cref="Module"/> that configure the Azure Servicebus as a command bus as both
	/// <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver{TAuthenticationToken}"/>.
	/// </summary>
	protected virtual IEnumerable<Module> GetLoggerModules(IServiceCollection services)
	{
		var list = new List<Module> { };

		return list;
	}
}