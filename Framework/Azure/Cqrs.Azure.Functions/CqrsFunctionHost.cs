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

using Cqrs.Azure.ConfigurationManager;
using Cqrs.DependencyInjection;
using Cqrs.Hosts;
using Cqrs.Authentication;
using Cqrs.Azure.Functions.Configuration;
using Cqrs.DependencyInjection.Modules;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#if NET48
#else
#endif

namespace Cqrs.Azure.Functions
{
	/// <summary>
	/// Execute command and event handlers in an Azure WebJob
	/// </summary>
	public class CqrsFunctionHost<TAuthenticationToken, TAuthenticationTokenHelper, TIsolatedFunctionHostModule>
		: TelemetryCoreHost<TAuthenticationToken>
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
		where TIsolatedFunctionHostModule : FunctionHostModule, new()
	{
		IServiceCollection hostServices { get; set; }

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
		public virtual void Go(IServiceCollection services)
		{
			hostServices = services;

			CoreHost = this;
			StartHost();
			Logger.LogInfo("Application Up and Ready.");
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="CqrsFunctionHost{TAuthenticationToken, TAuthenticationTokenHelper, TIsolatedFunctionHostModule}"/>
		/// </summary>
		public CqrsFunctionHost()
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
		public static void PrepareConfigurationManager(string rootPath, IConfigurationRoot config)
		{
			Cqrs.Configuration.IConfigurationManager configurationManager;
#if NET48
			configurationManager = new CloudConfigurationManager();
			DependencyResolver.ConfigurationManager = _configurationManager;
#else
			configurationManager = new CloudConfigurationManager(config);
			SetExecutionPath(config, rootPath);
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
		}

		/// <summary>
		/// Start the host post preparing and registering handlers.
		/// Then calls <see cref="HostingAbstractionsHostExtensions.Run"/> on the <see cref="IHost"/>.
		/// </summary>
		protected override void Start()
		{
			base.Start();
		}
		/// <summary>
		/// Sets the execution path
		/// </summary>
		public static void SetExecutionPath
		(
#if NET6_0
			Microsoft.Extensions.Configuration.IConfigurationRoot config, 
#endif
			string rootPath
		)
		{
#if NET6_0
			SetConfigurationManager(config);
#endif

			Cqrs.Configuration.ConfigurationExtensions.GetExecutionPath = () => Path.Combine(rootPath);
			HasSetExecutionPath = true;
		}

		/// <summary>
		/// Configure the <see cref="DependencyResolver"/>.
		/// </summary>
		protected override void ConfigureDefaultDependencyResolver()
		{
			foreach (Module supplementaryModule in GetSupplementaryModules(hostServices))
				DependencyResolver.ModulesToLoad.Add(supplementaryModule);

			DependencyResolver.Start(hostServices, prepareProvidedKernel: true);

			((DependencyResolver)Cqrs.Configuration.DependencyResolver.Current).SetKernel(hostServices.BuildServiceProvider());
		}

		/// <summary>
		/// A collection of <see cref="Module"/> that are required to be loaded
		/// </summary>
		protected virtual IEnumerable<Module> GetSupplementaryModules(IServiceCollection services)
		{
			var results = new List<Module>
			{
				new TIsolatedFunctionHostModule(),
#if NET6_0
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
}