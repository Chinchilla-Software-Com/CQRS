#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using Chinchilla.Logging.Azure.Storage;
using Chinchilla.Logging.Configuration;
using Cqrs.Configuration;
using Cqrs.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cqrs.DependencyInjection.Azure.Storage.Configuration
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="TableStorageLogger{TEntity}"/> as the <see cref="ILogger"/> and other require components.
	/// </summary>
	/// <typeparam name="TTableStorageLogger">The <see cref="Type"/> of the <see cref="TableStorageLogger"/> to wire up.</typeparam>
	public class AzureTableStorageLoggerModule<TTableStorageLogger>
		: ResolvableModule
		where TTableStorageLogger : class, ILogger
	{
		/// <summary>
		/// Indicates that web based wire-up is required rather than console, WPF or winforms based wire-up.
		/// </summary>
		protected bool SetupForWeb { get; private set; }

		/// <summary>
		/// Instantiate a new instance of the <see cref="AzureTableStorageLoggerModule{TTableStorageLogger}"/> that uses the provided <paramref name="configurationManager"/>
		/// to read the following configuration settings:
		/// "Cqrs.SetupForWeb": If set to true the system will be configured for hosting in IIS or some other web-server that provides access to System.Web.HttpContext.Current.
		/// </summary>
		/// <param name="configurationManager">The <see cref="IConfigurationManager"/> to use, if one isn't provided then <see cref="Cqrs.Configuration.DependencyResolver.ConfigurationManager"/> will be used, unless that isn't set, in which case <see cref="ConfigurationManager"/> is instantiate, used and then disposed.</param>
		public AzureTableStorageLoggerModule(IConfigurationManager configurationManager = null)
		{
			configurationManager = configurationManager ?? Cqrs.Configuration.DependencyResolver.ConfigurationManager ?? new ConfigurationManager();
			if (configurationManager.TryGetSetting("Cqrs.SetupForWeb", out bool setupForWeb))
				SetupForWeb = setupForWeb;
		}

		#region Overrides of ResolvableModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load(IServiceCollection services)
		{
			RegisterLoggerComponents(services);
		}

		#endregion

		/// <summary>
		/// Register the all components for the <see cref="ILogger"/>
		/// </summary>
		public virtual void RegisterLoggerComponents(IServiceCollection services)
		{
			bool isCorrelationIdHelperBound = IsRegistered<ICorrelationIdHelper>(services);
			if (!isCorrelationIdHelperBound)
			{
				if (SetupForWeb)
					services.AddSingleton<ICorrelationIdHelper, WebCorrelationIdHelper>();
				else
					services.AddSingleton<ICorrelationIdHelper, CorrelationIdHelper>();
			}

			bool isLoggerSettingsBound = IsRegistered<ILoggerSettings>(services);
			if (!isLoggerSettingsBound)
			{
				services.AddSingleton<ILoggerSettings, LoggerSettings>();
			}

			bool isTelemetryHelperBound = IsRegistered<ITelemetryHelper>(services);
			if (!isTelemetryHelperBound)
			{
				services.AddSingleton<ITelemetryHelper, NullTelemetryHelper>();
			}

			bool isLoggerBound = IsRegistered<ILogger>(services);
			if (isLoggerBound)
				services.RemoveAll<ILogger>();

			TTableStorageLogger logger = Resolve<TTableStorageLogger>(services);
			services.AddSingleton<ILogger>(logger);
		}
	}

	/// <summary>
	/// A <see cref="AzureTableStorageLoggerModule{TTableStorageLogger}"/> that wires up <see cref="TableStorageLogger"/> as the <see cref="ILogger"/> and other require components.
	/// </summary>
	public class AzureTableStorageLoggerModule
		: AzureTableStorageLoggerModule<TableStorageLogger>
	{

		/// <summary>
		/// Instantiate a new instance of the <see cref="AzureTableStorageLoggerModule"/> that uses the provided <paramref name="configurationManager"/>
		/// to read the following configuration settings:
		/// "Cqrs.SetupForWeb": If set to true the system will be configured for hosting in IIS or some other web-server that provides access to System.Web.HttpContext.Current.
		/// </summary>
		/// <param name="configurationManager">The <see cref="IConfigurationManager"/> to use, if one isn't provided then <see cref="Cqrs.Configuration.DependencyResolver.ConfigurationManager"/> will be used, unless that isn't set, in which case <see cref="ConfigurationManager"/> is instantiate, used and then disposed.</param>
		public AzureTableStorageLoggerModule(IConfigurationManager configurationManager = null)
			: base(configurationManager)
		{
		}
	}
}