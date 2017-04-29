#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Akka.Commands;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Cqrs.Bus;
using Cqrs.Configuration;
using Ninject.Modules;
using Ninject.Parameters;

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

			BusRegistrar.GetEventHandlerRegistrar = messageType =>
			{
				bool isAnActor = messageType.GetNestedTypes().Any(type => type.Name == "Actor");
				IEventHandlerRegistrar eventHandlerRegistrar = null;
				if (isAnActor)
					eventHandlerRegistrar = Resolve<IAkkaEventPublisher<TAuthenticationToken>>() as IEventHandlerRegistrar;
				return eventHandlerRegistrar ?? Resolve<IEventHandlerRegistrar>();
			};

			BusRegistrar.GetCommandHandlerRegistrar = messageType =>
			{
				bool isAnActor = messageType.GetNestedTypes().Any(type => type.Name == "Actor");
				ICommandHandlerRegistrar commandHandlerRegistrar = null;
				if (isAnActor)
					commandHandlerRegistrar = Resolve<IAkkaCommandSender<TAuthenticationToken>>() as ICommandHandlerRegistrar;
				return commandHandlerRegistrar ?? Resolve<ICommandHandlerRegistrar>();
			};
		}

		#endregion

		protected T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		protected object Resolve(Type serviceType)
		{
			return Kernel.Resolve(Kernel.CreateRequest(serviceType, null, new Parameter[0], true, true)).SingleOrDefault();
		}
	}
}