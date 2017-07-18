#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.Logging;
using cdmdotnet.Logging.Azure.ApplicationInsights;
using cdmdotnet.Logging.Azure.Configuration;
using cdmdotnet.Logging.Configuration;
using cdmdotnet.StateManagement;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Configuration;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using System;
using System.Web;
using cdmdotnet.StateManagement.Web;
using Cqrs.Authentication;
using Cqrs.Ninject.Configuration;
using Cqrs.Services;

namespace Cqrs.Ninject.Azure.Wcf.Configuration
{
	/// <summary>
	/// The core <see cref="INinjectModule"/> for use defining base level requirements.
	/// </summary>
	public class WebHostModule : ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterBasicHelpers();
			RegisterAzureConfigurations();
			RegisterBasicSerives();
			RegisterWebBit();
		}

		#endregion

		/// <summary>
		/// Register the all Azure configurations
		/// </summary>
		protected virtual void RegisterAzureConfigurations()
		{
			Bind<ILoggerSettings>()
				.To<AzureLoggerSettingsConfiguration>()
				.InSingletonScope();

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

			Bind<ITelemetryHelper>()
				.To<TelemetryHelper>()
				.InSingletonScope();
		}
		/// <summary>
		/// Registers the <see cref="IContextItemCollectionFactory"/> required.
		/// </summary>
		protected virtual void RegisterContextItemCollectionFactory()
		{
			// We use console state as, even though a webjob runs in an azure website, it's technically loaded via something call the 'WindowsScriptHost', which is not web and IIS based so it's threading model is very different and more console based.
			Bind<IContextItemCollectionFactory>()
				.To<WebContextItemCollectionFactory>()
				.InSingletonScope();
		}

		/// <summary>
		/// Registers the basic services required.
		/// </summary>
		protected virtual void RegisterBasicSerives()
		{
			string authenticationType;
			if (!Resolve<IConfigurationManager>().TryGetSetting("Cqrs.AuthenticationTokenType", out authenticationType))
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
			Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
			Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
		}
	}
}