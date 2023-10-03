#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
#if NETSTANDARD2_0
using System.Linq;
using Microsoft.Extensions.Configuration;
#endif
using Cqrs.Authentication;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration;
using Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration;
using Cqrs.Ninject.Azure.Wcf.Configuration;
using Cqrs.Ninject.Configuration;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.Wcf
{
	/// <summary>
	/// Execute command and event handlers in a WCF Host using Ninject, defaulting to <see cref="WebHostModule"/> as the module to load.
	/// </summary>
	public class CqrsWebHost<TAuthenticationToken, TAuthenticationTokenHelper>
		: CqrsWebHost<TAuthenticationToken, TAuthenticationTokenHelper, WebHostModule>
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
	{
	}

	/// <summary>
	/// Execute command and event handlers in a WCF Host using Ninject
	/// </summary>
	public class CqrsWebHost<TAuthenticationToken, TAuthenticationTokenHelper, TWebHostModule>
		: TelemetryCoreHost<TAuthenticationToken>
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
		where TWebHostModule : WebHostModule, new ()
	{
#if NETSTANDARD2_0
		/// <summary>
		/// Set the <see cref="IConfigurationRoot"/> on <see cref="Cqrs.Configuration.ConfigurationManager.Configuration"/> and prepare a <see cref="CloudConfigurationManager"/>
		/// </summary>
		public static new void SetConfigurationManager(IConfigurationRoot configuration)
		{
			Cqrs.Configuration.ConfigurationManager.Configuration = configuration;
			_configurationManager = new CloudConfigurationManager(Cqrs.Configuration.ConfigurationManager.Configuration);
		}
#endif

		#region Overrides of CoreHost

		/// <summary>
		/// Configure the <see cref="DependencyResolver"/>.
		/// </summary>
		protected override void ConfigureDefaultDependencyResolver()
		{
			foreach (INinjectModule supplementaryModule in GetSupplementaryModules())
				NinjectDependencyResolver.ModulesToLoad.Add(supplementaryModule);

			NinjectDependencyResolver.Start(prepareProvidedKernel: true);
		}

#endregion

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> that are required to be loaded
		/// </summary>
		protected virtual IEnumerable<INinjectModule> GetSupplementaryModules()
		{
			var results = new List<INinjectModule>
			{
				new TWebHostModule(),
#if NETSTANDARD2_0
				new CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper>(new CloudConfigurationManager(Cqrs.Configuration.ConfigurationManager.Configuration))
#else
				new CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper>(new CloudConfigurationManager())
#endif
			};

			results.AddRange(GetCommandBusModules());
			results.AddRange(GetEventBusModules());
			results.AddRange(GetEventStoreModules());

			return results;
		}

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> that configure the Azure Servicebus as a command bus as both
		/// <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IEnumerable<INinjectModule> GetCommandBusModules()
		{
			var list = new List<INinjectModule> { new AzureCommandBusPublisherModule<TAuthenticationToken>() };
			bool setting;

			if (!ConfigurationManager.TryGetSetting("Cqrs.Hosts.EnableCommandReceiving", out setting))
				setting = true;
			if (setting)
				list.Add(new AzureCommandBusReceiverModule<TAuthenticationToken>());

			return list;
		}

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> that configure the Azure Servicebus as a event bus as both
		/// <see cref="IEventPublisher{TAuthenticationToken}"/> and <see cref="IEventReceiver{TAuthenticationToken}"/>
		/// If the app setting Cqrs.Host.EnableEventReceiving is "false" then no modules will be returned.
		/// </summary>
		protected virtual IEnumerable<INinjectModule> GetEventBusModules()
		{
			var list = new List<INinjectModule> { new AzureEventBusPublisherModule<TAuthenticationToken>() };
			bool setting;

			if (!ConfigurationManager.TryGetSetting("Cqrs.Hosts.EnableEventReceiving", out setting))
				setting = true;
			if (setting)
				list.Add(new AzureEventBusReceiverModule<TAuthenticationToken>());

			return list;
		}

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> that configure SQL server as the <see cref="IEventStore{TAuthenticationToken}"/>
		/// </summary>
		protected virtual IEnumerable<INinjectModule> GetEventStoreModules()
		{
			return new List<INinjectModule>
			{
				new SimplifiedSqlModule<TAuthenticationToken>()
			};
		}

		/// <summary>
		/// Prepare the host before registering handlers and starting the host.
		/// </summary>
		protected override void RunStartUp()
		{
			base.RunStartUp();

			if (false && TelemetryClient != null)
			{
				var telemetryHelper = NinjectDependencyResolver.Current.Resolve<Chinchilla.Logging.Azure.ApplicationInsights.TelemetryHelper>();
				System.Reflection.PropertyInfo property = telemetryHelper
					.GetType()
					.GetProperty("TelemetryClient", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.NonPublic);
				property.SetValue(telemetryHelper, TelemetryClient);
			}

		}
	}
}