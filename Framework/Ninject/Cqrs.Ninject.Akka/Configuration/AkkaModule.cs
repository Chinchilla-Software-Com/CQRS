#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using System.Reflection;
using Cqrs.Akka.Commands;
using Cqrs.Akka.Domain;
using Cqrs.Akka.Events;
using Cqrs.Akka.Snapshots;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Ninject.Configuration;
using Cqrs.Snapshots;
using Ninject.Modules;

namespace Cqrs.Ninject.Akka.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up many of the prerequisites for running CQRS.NET with Akka.NET
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AkkaModule<TAuthenticationToken> : ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			Bind<IAkkaAggregateRepository<TAuthenticationToken>>().To<AkkaAggregateRepository<TAuthenticationToken>>().InSingletonScope();
			Bind<IAkkaSagaRepository<TAuthenticationToken>>().To<AkkaSagaRepository<TAuthenticationToken>>().InSingletonScope();
			Bind<IAkkaEventPublisher<TAuthenticationToken>>().To<AkkaEventBus<TAuthenticationToken>>().InSingletonScope();
			Bind<IAkkaEventPublisherProxy<TAuthenticationToken>>().To<AkkaEventBusProxy<TAuthenticationToken>>().InSingletonScope();
			Bind<IAkkaCommandPublisher<TAuthenticationToken>>().To<AkkaCommandBus<TAuthenticationToken>>().InSingletonScope();
			Bind<IAkkaCommandPublisherProxy<TAuthenticationToken>>().To<AkkaCommandBusProxy<TAuthenticationToken>>().InSingletonScope();

			Bind<IAkkaSnapshotAggregateRepository<TAuthenticationToken>>().To<AkkaSnapshotRepository<TAuthenticationToken>>().InSingletonScope();
			Rebind<ISnapshotStrategy<TAuthenticationToken>>().To<DefaultAkkaSnapshotStrategy<TAuthenticationToken>>().InSingletonScope();

			BusRegistrar.GetEventHandlerRegistrar = (messageType, handlerDelegateTargetedType) =>
			{
				bool isAnActor = messageType != null && messageType.GetNestedTypes().Any(type => type.Name == "Actor");
				IEventHandlerRegistrar eventHandlerRegistrar = null;
				if (isAnActor)
					eventHandlerRegistrar = Resolve<IAkkaEventPublisher<TAuthenticationToken>>() as IEventHandlerRegistrar;
				return eventHandlerRegistrar ?? Resolve<IEventHandlerRegistrar>();
			};

			BusRegistrar.GetCommandHandlerRegistrar = (messageType, handlerDelegateTargetedType) =>
			{
				bool isAnActor = handlerDelegateTargetedType != null && handlerDelegateTargetedType.GetProperty("AggregateResolver", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public) != null;
				ICommandHandlerRegistrar commandHandlerRegistrar = null;
				if (isAnActor)
					commandHandlerRegistrar = Resolve<IAkkaCommandPublisher<TAuthenticationToken>>() as ICommandHandlerRegistrar;
				return commandHandlerRegistrar ?? Resolve<ICommandHandlerRegistrar>();
			};
		}

		#endregion
	}
}