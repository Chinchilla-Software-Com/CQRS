#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Akka.Commands;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Akka.Configuration
{
	public class AkkaModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			Bind<IAkkaRepository<TAuthenticationToken>>().To<AkkaRepository<TAuthenticationToken>>().InSingletonScope();
			Bind<IAkkaEventPublisher<TAuthenticationToken>>().To<AkkaEventBus<TAuthenticationToken>>().InSingletonScope();
			Bind<IAkkaEventPublisherProxy<TAuthenticationToken>>().To<AkkaEventBusProxy<TAuthenticationToken>>().InSingletonScope();
			Bind<IAkkaCommandSender<TAuthenticationToken>>().To<AkkaCommandBus<TAuthenticationToken>>().InSingletonScope();
		}

		#endregion
	}
}