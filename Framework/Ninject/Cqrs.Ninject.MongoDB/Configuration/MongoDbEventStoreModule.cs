#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Events;
using Cqrs.MongoDB.Events;
using Cqrs.MongoDB.Serialisers;
using Cqrs.Snapshots;
using Ninject.Modules;

namespace Cqrs.Ninject.MongoDB.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="MongoDbEventStore{TAuthenticationToken}"/> as the <see cref="IEventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class MongoDbEventStoreModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterFactories();
			RegisterEventSerialisationConfiguration();
			RegisterEventStore();
		}

		#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
			Bind<IMongoDbEventStoreConnectionStringFactory>()
				.To<MongoDbEventStoreConnectionStringFactory>()
				.InSingletonScope();
			Bind<IMongoDbSnapshotStoreConnectionStringFactory>()
				.To<MongoDbSnapshotStoreConnectionStringFactory>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the all event serialisation configurations
		/// </summary>
		public virtual void RegisterEventSerialisationConfiguration()
		{
			Bind<IEventBuilder<TAuthenticationToken>>()
				.To<MongoDbEventBuilder<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<IEventDeserialiser<TAuthenticationToken>>()
				.To<MongoDbEventDeserialiser<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<ISnapshotDeserialiser>()
				.To<MongoDbSnapshotDeserialiser>()
				.InSingletonScope();

			if (Kernel.GetBindings(typeof(ISnapshotBuilder)).Any())
				Kernel.Unbind<ISnapshotBuilder>();
			Bind<ISnapshotBuilder>()
				.To<MongoDbSnapshotBuilder>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the <see cref="IEventStore{TAuthenticationToken}"/>
		/// </summary>
		public virtual void RegisterEventStore()
		{
			Bind<IEventStore<TAuthenticationToken>>()
				.To<MongoDbEventStore<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<ISnapshotStore>()
				.To<MongoDbSnapshotStore>()
				.InSingletonScope();
		}
	}
}