using System;
using Cqrs.EventStore;
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="EventStore.EventStore{TAuthenticationToken}"/> as the <see cref="IEventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class EventStoreModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterFactories();
			RegisterServices();
			RegisterCqrsRequirements();
		}

		#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
			Bind<EventStore.IEventBuilder<TAuthenticationToken>>()
				.To<EventFactory<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<EventStore.IEventDeserialiser<TAuthenticationToken>>()
				.To<EventFactory<TAuthenticationToken>>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices()
		{
		}

		/// <summary>
		/// Register the <see cref="IEventStoreConnectionHelper"/> and <see cref="IEventStore{TAuthenticationToken}"/>.
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
			Bind<IEventStoreConnectionHelper>()
				.To<EventStoreConnectionHelper<TAuthenticationToken>>()
				.InSingletonScope();

			Bind<IEventStore<TAuthenticationToken>>()
				.To<EventStore.EventStore<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}