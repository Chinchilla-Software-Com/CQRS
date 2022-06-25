#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Chinchilla.Logging;
using Chinchilla.Logging.Azure.ApplicationInsights;
using Chinchilla.Logging.Azure.Configuration;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement;
using Chinchilla.StateManagement.Threaded;
using Chinchilla.StateManagement.Web;
using Cqrs.Authentication;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Configuration;
using Cqrs.Ninject.Configuration;
using Cqrs.Services;
using Ninject.Modules;
#if NET472
using Ninject.Web.Common;
using System.Web;
using Ninject;
#endif
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Ninject.Azure.Wcf.Configuration
{
	/// <summary>
	/// The core <see cref="INinjectModule"/> for use defining base level requirements.
	/// </summary>
	public class WebHostModule : ResolvableModule
	{
#if NETSTANDARD2_0
		/// <summary>
		/// Gets or sets the <see cref="IConfiguration"/>. This must be set manually as dependency injection may not be ready in-time.
		/// </summary>
		public static IConfiguration Configuration { get; set; }
#endif

		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterBasicHelpers();
			RegisterAzureConfigurations();
			RegisterBasicServices();
			RegisterWebBit();
		}

#endregion

		/// <summary>
		/// Register the all Azure configurations
		/// </summary>
		protected virtual void RegisterAzureConfigurations()
		{
			var loggerSettingsBindings = Kernel.GetBindings(typeof(ILoggerSettings)).ToList();
			bool isLoggerSettingsBound = loggerSettingsBindings.Any();
			if (!isLoggerSettingsBound)
			{
				Bind<ILoggerSettings>()
					.To<AzureLoggerSettingsConfiguration>()
					.InSingletonScope();
			}
			else
			{
				var obj = Resolve<ILoggerSettings>();
				if (!(obj is AzureLoggerSettingsConfiguration))
				{
					Unbind<ILoggerSettings>();
					Bind<ILoggerSettings>()
						.To<AzureLoggerSettingsConfiguration>()
						.InSingletonScope();
				}
			}

#if NETSTANDARD2_0
			Bind<IConfiguration>()
				.ToConstant(Configuration)
				.InSingletonScope();
#endif

			Bind<IConfigurationManager>()
				.To<CloudConfigurationManager>()
				.InSingletonScope();
		}

		/// <summary>
		/// Registers the basic helpers required.
		/// </summary>
		protected virtual void RegisterBasicHelpers()
		{
			RegisterContextItemCollectionFactory();

			var telemetryHelperBindings = Kernel.GetBindings(typeof(ITelemetryHelper)).ToList();
			bool isTelemetryHelperBound = telemetryHelperBindings.Any();
			if (!isTelemetryHelperBound)
			{
				Bind<ITelemetryHelper>()
					.To<TelemetryHelper>()
					.InSingletonScope();
			}
			else
			{
				var obj = Resolve<ITelemetryHelper>();
				if (!(obj is TelemetryHelper))
				{
					Unbind<ITelemetryHelper>();
					Bind<ITelemetryHelper>()
						.To<TelemetryHelper>()
						.InSingletonScope();
				}
			}
		}
		/// <summary>
		/// Registers the <see cref="IContextItemCollectionFactory"/> required.
		/// </summary>
		protected virtual void RegisterContextItemCollectionFactory()
		{
			Bind<IContextItemCollectionFactory>()
				.To<WebContextItemCollectionFactory>()
				.InSingletonScope();
			Bind<IContextItemCollection>()
				.To<WebContextItemCollection>()
				.InSingletonScope();
		}

		/// <summary>
		/// Registers the basic services required.
		/// </summary>
		protected virtual void RegisterBasicServices()
		{
			string authenticationType;
			if (!Resolve<IConfigurationManager>().TryGetSetting("Cqrs.AuthenticationTokenType", out authenticationType) || string.IsNullOrWhiteSpace(authenticationType))
				authenticationType = "Guid";

			if (authenticationType.ToLowerInvariant() == "int" || authenticationType.ToLowerInvariant() == "integer")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<int>>()
					.InThreadScope();
			else if (authenticationType.ToLowerInvariant() == "guid")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<Guid>>()
					.InThreadScope();
			else if (authenticationType.ToLowerInvariant() == "string" || authenticationType.ToLowerInvariant() == "text")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<string>>()
					.InThreadScope();

			else if (authenticationType == "SingleSignOnToken")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<SingleSignOnToken>>()
					.InThreadScope();
			else if (authenticationType == "SingleSignOnTokenWithUserRsn")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<SingleSignOnTokenWithUserRsn>>()
					.InThreadScope();
			else if (authenticationType == "SingleSignOnTokenWithCompanyRsn")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<SingleSignOnTokenWithCompanyRsn>>()
					.InThreadScope();
			else if (authenticationType == "SingleSignOnTokenWithUserRsnAndCompanyRsn")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<SingleSignOnTokenWithUserRsnAndCompanyRsn>>()
					.InThreadScope();

			else if (authenticationType == "ISingleSignOnToken")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<ISingleSignOnToken>>()
					.InThreadScope();
			else if (authenticationType == "ISingleSignOnTokenWithUserRsn")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<ISingleSignOnTokenWithUserRsn>>()
					.InThreadScope();
			else if (authenticationType == "ISingleSignOnTokenWithCompanyRsn")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<ISingleSignOnTokenWithCompanyRsn>>()
					.InThreadScope();
			else if (authenticationType == "ISingleSignOnTokenWithUserRsnAndCompanyRsn")
				Bind<IUnitOfWorkService>()
					.To<UnitOfWorkService<ISingleSignOnTokenWithUserRsnAndCompanyRsn>>()
					.InThreadScope();
		}

		/// <summary>
		/// Registers some things Ninject likes.
		/// </summary>
		protected virtual void RegisterWebBit()
		{
#if NET472
			Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
			Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
#endif
		}
	}
}