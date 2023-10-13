using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.Azure.Functions.Isolated;
using Cqrs.Azure.Functions.Isolated.Configuration;
using Cqrs.DependencyInjection.Azure.ServiceBus.CommandBus;
using Cqrs.DependencyInjection.Azure.ServiceBus.EventBus;
using Cqrs.DependencyInjection.Modules;
using Cqrs.Scheduler.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

public class Program
	: CqrsIsolatedFunctionHost<Guid, DefaultAuthenticationTokenHelper, IsolatedFunctionHostModule>
{
	/// <summary>
	/// Entry point for the issolated application.
	/// </summary>
	public static void Main(string[] args)
	{
		PrepareConfigurationManager();
		new Program().Go();
	}

	protected override Type[] GetCommandOrEventHandlerTypes()
	{
		return new[] { typeof(PublishTimeZonesCommandHandler) };
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
		var list = new List<Module> { new AzureCommandBusPublisherModule<Guid>() };
		bool setting;

		if (!ConfigurationManager.TryGetSetting("Cqrs.Hosts.EnableCommandReceiving", out setting))
			setting = true;
		if (setting)
			list.Add(new AzureCommandBusReceiverModule<Guid>());

		return list;
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
			list.Add(new AzureEventBusReceiverModule<Guid>());

		return list;
	}

}