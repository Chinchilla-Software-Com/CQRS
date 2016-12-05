#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Web;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.WebApi.SignalR.Hubs;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;

namespace $safeprojectname$.Configuration
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

			RegisterLogger();
			RegisterWebApi();
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

			Bind<ILoggerSettings>()
				.To<LoggerSettings>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the some WebAPI and SignalR requirements
		/// </summary>
		protected virtual void RegisterWebApi()
		{
			Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
			Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

			Bind<INotificationHub>()
				.To<NotificationHub>()
				.InSingletonScope();

			Bind<SingleSignOnTokenFactory<SingleSignOnToken>>()
				.To<DefaultSingleSignOnTokenFactory>()
				.InSingletonScope();
		}
	}
}