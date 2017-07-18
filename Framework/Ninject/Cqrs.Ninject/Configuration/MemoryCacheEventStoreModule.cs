#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class MemoryCacheEventStoreModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterEventSerialisationConfiguration();
			RegisterEventStore();
		}

		#endregion

		/// <summary>
		/// Register the all event serialisation configurations
		/// </summary>
		public virtual void RegisterEventSerialisationConfiguration()
		{
			Bind<IEventBuilder<TAuthenticationToken>>()
				.To<DefaultEventBuilder<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<IEventDeserialiser<TAuthenticationToken>>()
				.To<EventDeserialiser<TAuthenticationToken>>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the <see cref="IEventStore{TAuthenticationToken}"/>
		/// </summary>
		public virtual void RegisterEventStore()
		{
			Bind<IEventStore<TAuthenticationToken>>()
				.To<MemoryCacheEventStore<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}