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
using Cqrs.Authentication;
using Cqrs.Azure.ConfigurationManager;
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
				.To<CloudConfigurationManager>()
				.InSingletonScope();

			RegisterLogger();
			RegisterWebApi();
		}

		/// <summary>
		/// Register the <see cref="ILogger"/>
		/// </summary>
		protected virtual void RegisterLogger()
		{
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