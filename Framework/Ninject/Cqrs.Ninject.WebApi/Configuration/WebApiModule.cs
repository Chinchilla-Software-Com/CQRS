using System;
using System.Web;
using Cqrs.Configuration;
using Cqrs.WebApi.SignalR.Hubs;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;

namespace Cqrs.Ninject.WebApi.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that sets up default WebApi.
	/// </summary>
	public class WebApiModule : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			Bind<IConfigurationManager>()
				.To<ConfigurationManager>()
				.InSingletonScope();

			RegisterWebApi();
		}

		#endregion

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
		}
	}
}
