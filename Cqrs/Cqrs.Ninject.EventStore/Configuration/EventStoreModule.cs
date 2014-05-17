using Cqrs.Authentication;
using Cqrs.EventStore;
using Cqrs.EventStore.Bus;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class EventStoreModule<TAuthenticationToken, TAuthenticationTokenHelper> : CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper>
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
	{
		/// <summary>
		/// Register the all Cqrs command handlers
		/// </summary>
		public override void RegisterCqrsRequirements()
		{
			base.RegisterCqrsRequirements();

			Bind<IStoreLastEventProcessed>()
				.To<FileBasedLastEventProcessedStore>()
				.InSingletonScope();

			Bind<IEventBuilder<TAuthenticationToken>>()
			.To<EventFactory<TAuthenticationToken>>()
			.InSingletonScope();
			Bind<IEventDeserialiser<TAuthenticationToken>>()
				.To<EventFactory<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<IEventStoreConnectionHelper>()
				.To<EventStoreConnectionHelper<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}