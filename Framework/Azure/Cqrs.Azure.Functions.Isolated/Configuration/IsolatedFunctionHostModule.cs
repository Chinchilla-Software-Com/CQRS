#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using Chinchilla.Logging.Azure.ApplicationInsights;
using Chinchilla.Logging.Azure.Configuration;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Authentication;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Configuration;
using Cqrs.Services;
using Cqrs.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

#if NET6_0
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Azure.Functions.Isolated.Configuration
{
	/// <summary>
	/// The core <see cref="Module"/> for use defining base level requirements.
	/// </summary>
	public class IsolatedFunctionHostModule : ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load(IServiceCollection services)
		{
			RegisterBasicHelpers(services);
			RegisterAzureConfigurations(services);
			RegisterBasicServices(services);
			RegisterWebBit(services);
		}

#endregion

		/// <summary>
		/// Register the all Azure configurations
		/// </summary>
		protected virtual void RegisterAzureConfigurations(IServiceCollection services)
		{
			bool isLoggerSettingsBound = IsRegistered<ILoggerSettings>(services);
			if (!isLoggerSettingsBound)
			{
				services.AddSingleton<ILoggerSettings, AzureLoggerSettingsConfiguration>();
			}
			else
			{
				var obj = Resolve<ILoggerSettings>(services);
				if (!(obj is AzureLoggerSettingsConfiguration))
				{
					Unbind<ILoggerSettings>(services);
					services.AddSingleton<ILoggerSettings,AzureLoggerSettingsConfiguration>();
				}
			}

#if NET6_0
			services.AddSingleton<IConfiguration>(Cqrs.Configuration.ConfigurationManager.BaseConfiguration);
#endif

			services.AddSingleton<IConfigurationManager, CloudConfigurationManager>();
			DependencyResolver.ConfigurationManager = Resolve<IConfigurationManager>(services);
		}

		/// <summary>
		/// Registers the basic helpers required.
		/// </summary>
		protected virtual void RegisterBasicHelpers(IServiceCollection services)
		{
			RegisterContextItemCollectionFactory(services);

			bool isTelemetryHelperBound = IsRegistered<ITelemetryHelper>(services);
			if (!isTelemetryHelperBound)
			{
				services.AddSingleton<ITelemetryHelper, TelemetryHelper>();
			}
			else
			{
				var obj = Resolve<ITelemetryHelper>(services);
				if (!(obj is TelemetryHelper))
				{
					Unbind<ITelemetryHelper>(services);
					services.AddSingleton<ITelemetryHelper, TelemetryHelper>();
				}
			}
		}
		/// <summary>
		/// Registers the <see cref="IContextItemCollectionFactory"/> required.
		/// </summary>
		protected virtual void RegisterContextItemCollectionFactory(IServiceCollection services)
		{
			services.AddSingleton<IContextItemCollectionFactory, ContextItemCollectionFactory>();
			services.AddSingleton<IContextItemCollection, Chinchilla.StateManagement.Threaded.ContextItemCollection>();
		}

		/// <summary>
		/// Registers the basic services required.
		/// </summary>
		protected virtual void RegisterBasicServices(IServiceCollection services)
		{
			string authenticationType;
			if (!Resolve<IConfigurationManager>(services).TryGetSetting("Cqrs.AuthenticationTokenType", out authenticationType) || string.IsNullOrWhiteSpace(authenticationType))
				authenticationType = "Guid";

			if (authenticationType.ToLowerInvariant() == "int" || authenticationType.ToLowerInvariant() == "integer")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<int>>();
			else if (authenticationType.ToLowerInvariant() == "guid")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<Guid>>();
			else if (authenticationType.ToLowerInvariant() == "string" || authenticationType.ToLowerInvariant() == "text")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<string>>();

			else if (authenticationType == "SingleSignOnToken")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<SingleSignOnToken>>();
			else if (authenticationType == "SingleSignOnTokenWithUserRsn")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<SingleSignOnTokenWithUserRsn>>();
			else if (authenticationType == "SingleSignOnTokenWithCompanyRsn")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<SingleSignOnTokenWithCompanyRsn>>();
			else if (authenticationType == "SingleSignOnTokenWithUserRsnAndCompanyRsn")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<SingleSignOnTokenWithUserRsnAndCompanyRsn>>();

			else if (authenticationType == "ISingleSignOnToken")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<ISingleSignOnToken>>();
			else if (authenticationType == "ISingleSignOnTokenWithUserRsn")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<ISingleSignOnTokenWithUserRsn>>();
			else if (authenticationType == "ISingleSignOnTokenWithCompanyRsn")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<ISingleSignOnTokenWithCompanyRsn>>();
			else if (authenticationType == "ISingleSignOnTokenWithUserRsnAndCompanyRsn")
				services.AddScoped<IUnitOfWorkService, UnitOfWorkService<ISingleSignOnTokenWithUserRsnAndCompanyRsn>>();
		}

		/// <summary>
		/// Registers some things Ninject likes.
		/// </summary>
		protected virtual void RegisterWebBit(IServiceCollection services)
		{
		}
	}
}