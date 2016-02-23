using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Ninject.Modules;

namespace MyCompany.MyProject.Configuration
{
	public class BaseModule : NinjectModule
	{
		public override void Load()
		{
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
