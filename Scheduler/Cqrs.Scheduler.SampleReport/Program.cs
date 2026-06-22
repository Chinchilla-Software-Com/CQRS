using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.Azure.Functions;
using Cqrs.Azure.Functions.Configuration;
using Cqrs.Azure.Functions.ServiceBus;
using Cqrs.DependencyInjection.Azure.ServiceBus.EventBus;
using Cqrs.DependencyInjection.Modules;
using Cqrs.Scheduler.SampleReport.EventHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Program
	: CqrsFunctionHost<Guid, DefaultAuthenticationTokenHelper, FunctionHostModule>
{
	/// <summary>
	/// Entry point for the issolated application.
	/// </summary>
	public static void Main(string rootPath, IConfigurationRoot config, IServiceCollection services)
	{
		PrepareConfigurationManager(rootPath, config);
		new Program().Go(services);
	}

	protected override Type[] GetCommandOrEventHandlerTypes()
	{
		return new[] { typeof(SendAnEmailAtMidnight) };
	}

	/// <summary>
	/// A collection of <see cref="Module"/> that are required to be loaded
	/// </summary>
	protected override IEnumerable<Module> GetSupplementaryModules(IServiceCollection services)
	{
		List<Module> results = base.GetSupplementaryModules(services).ToList();

		results.AddRange(GetCommandBusModules(services));
		results.AddRange(GetEventBusModules(services));

		return results;
	}

	/// <summary>
	/// A collection of <see cref="Module"/> that configure the Azure Servicebus as a command bus as both
	/// <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver{TAuthenticationToken}"/>.
	/// </summary>
	protected virtual IEnumerable<Module> GetCommandBusModules(IServiceCollection services)
	{
		return Enumerable.Empty<Module>();
	}

	/// <summary>
	/// A collection of <see cref="Module"/> that configure the Azure Servicebus as a event bus as both
	/// <see cref="IEventPublisher{TAuthenticationToken}"/> and <see cref="IEventReceiver{TAuthenticationToken}"/>
	/// If the app setting Cqrs.Host.EnableEventReceiving is "false" then no modules will be returned.
	/// </summary>
	protected virtual IEnumerable<Module> GetEventBusModules(IServiceCollection services)
	{
		var list = new List<Module> { new AzureEventBusPublisherModule<Guid>() };
		bool setting;

		if (!ConfigurationManager.TryGetSetting("Cqrs.Hosts.EnableEventReceiving", out setting))
			setting = true;
		if (setting)
			list.Add(new AzureEventBusReceiverModule<Guid, AzureFunctionEventBusReceiver<Guid>>());

		return list;
	}

}