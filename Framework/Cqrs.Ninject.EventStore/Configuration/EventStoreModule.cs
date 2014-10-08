using Cqrs.EventStore;
using Cqrs.EventStore.Bus;
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
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
		/// Register the all Cqrs command handlers
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