#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Configuration;
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
		/// <summary>
		/// Indicates that the <see cref="ISnapshotStrategy{TAuthenticationToken}"/> should be registered automatically.
		/// </summary>
		protected bool RegisterMongoDbSnapshotDeserialiser { get; private set; }

		/// <summary>
		/// Indicates that the <see cref="ISnapshotBuilder"/> should be registered automatically.
		/// </summary>
		protected bool RegisterMongoDbSnapshotBuilder { get; private set; }

		/// <summary>
		/// Instantiate a new instance of the <see cref="MongoDbEventStoreModule{TAuthenticationToken}"/> that uses the provided <paramref name="configurationManager"/>
		/// to read the following configuration settings:
		/// "Cqrs.RegisterMongoDbSnapshotDeserialiser": If set true the <see cref="MongoDbSnapshotDeserialiser"/> will be registered.
		/// "Cqrs.RegisterMongoDbSnapshotBuilder": If set true the <see cref="MongoDbSnapshotBuilder"/> will be registered.
		/// </summary>
		/// <param name="configurationManager">The <see cref="IConfigurationManager"/> to use, if one isn't provided then <see cref="ConfigurationManager"/> is instantiate, used and then disposed.</param>
		public MongoDbEventStoreModule(IConfigurationManager configurationManager = null)
		{
			configurationManager = configurationManager ?? new ConfigurationManager();
			bool registerMongoDbSnapshotDeserialiser;
			if (configurationManager.TryGetSetting("Cqrs.RegisterMongoDbSnapshotDeserialiser", out registerMongoDbSnapshotDeserialiser))
				RegisterMongoDbSnapshotDeserialiser = registerMongoDbSnapshotDeserialiser;
			else
				RegisterMongoDbSnapshotDeserialiser = true;
			bool registerMongoDbSnapshotBuilder;
			if (configurationManager.TryGetSetting("Cqrs.RegisterMongoDbSnapshotBuilder", out registerMongoDbSnapshotBuilder))
				RegisterMongoDbSnapshotBuilder = registerMongoDbSnapshotBuilder;
			else
				RegisterMongoDbSnapshotBuilder = true;
		}

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

		/// <summary>
		/// Register the all Cqrs requirements
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
			if (RegisterMongoDbSnapshotDeserialiser)
				Rebind<ISnapshotDeserialiser>()
					.To<MongoDbSnapshotDeserialiser>()
					.InSingletonScope();

			if (RegisterMongoDbSnapshotBuilder)
				Rebind<ISnapshotBuilder>()
					.To<MongoDbSnapshotBuilder>()
					.InSingletonScope();
		}
	}
}