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
using cdmdotnet.StateManagement.Threaded;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Configuration;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.WebJobs.Configuration
{
	/// <summary>
	/// The core <see cref="INinjectModule"/> for use defining base level requirements.
	/// </summary>
	public class WebJobHostModule : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterBasicHelpers();
			RegisterAzureConfigurations();
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
			// We use console state as, even though a webjob runs in an azure website, it's technically loaded via something call the 'WindowsScriptHost', which is not web and IIS based so it's threading model is very different and more console based.
			Bind<IContextItemCollectionFactory>()
				.To<ThreadedContextItemCollectionFactory>()
				.InSingletonScope();

			Bind<ITelemetryHelper>()
				.To<TelemetryHelper>()
				.InSingletonScope();
		}
	}
}