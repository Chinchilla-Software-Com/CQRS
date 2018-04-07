#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Azure.DocumentDb;
using Cqrs.Azure.DocumentDb.Events;
using Cqrs.Events;
using Cqrs.Snapshots;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.DocumentDb.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureDocumentDbEventStore{TAuthenticationToken}"/> as the <see cref="IEventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureDocumentDbEventStoreModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterFactories();
			RegisterServices();
			RegisterEventStore();
			RegisterAzureHelpers();
		}

		#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
			Bind<IEventBuilder<TAuthenticationToken>>()
				.To<DefaultEventBuilder<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<IEventDeserialiser<TAuthenticationToken>>()
				.To<EventDeserialiser<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<ISnapshotDeserialiser>()
				.To<SnapshotDeserialiser>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices()
		{
		}

		/// <summary>
		/// Register <see cref="IAzureDocumentDbHelper"/> if it hasn't already been registered.
		/// </summary>
		public virtual void RegisterAzureHelpers()
		{
			if (!Kernel.GetBindings(typeof(IAzureDocumentDbHelper)).Any())
			{
				Bind<IAzureDocumentDbHelper>()
					.To<AzureDocumentDbHelper>()
					.InSingletonScope();
			}
		}

		/// <summary>
		/// Register the <see cref="IAzureDocumentDbEventStoreConnectionStringFactory"/> and <see cref="IEventStore{TAuthenticationToken}"/>
		/// </summary>
		public virtual void RegisterEventStore()
		{
			Bind<IAzureDocumentDbEventStoreConnectionStringFactory>()
				.To<AzureDocumentDbEventStoreConnectionStringFactory>()
				.InSingletonScope();
			Bind<IAzureDocumentDbSnapshotStoreConnectionStringFactory>()
				.To<AzureDocumentDbSnapshotStoreConnectionStringFactory>()
				.InSingletonScope();

			Bind<IEventStore<TAuthenticationToken>>()
				.To<AzureDocumentDbEventStore<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<ISnapshotStore>()
				.To<AzureDocumentDbSnapshotStore>()
				.InSingletonScope();
		}
	}
}