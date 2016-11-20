#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Configuration;
using Ninject.Modules;

namespace Northwind.Domain.Host.Web.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the domain package.
	/// </summary>
	public class WebHostModule : NinjectModule
	{
		public override void Load()
		{
			Bind<IConfigurationManager>()
				.To<ConfigurationManager>()
				.InSingletonScope();

			Bind<ILoggerSettings>()
				.To<LoggerSettingsConfigurationSection>()
				.InSingletonScope();

			RegisterLogger();
		}

		/// <summary>
		/// Register the <see cref="ILogger"/>
		/// </summary>
		protected virtual void RegisterLogger()
		{
			Kernel.Unbind<ILogger>();

			Bind<ILogger>()
				.To<ConsoleLogger>()
				.InSingletonScope();
		}
	}
}