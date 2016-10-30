#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Linq;
using Cqrs.Azure.DocumentDb;
using Cqrs.Azure.DocumentDb.Events;
using Cqrs.Events;
using Cqrs.Ninject.Azure.DocumentDb.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.DocumentDb.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class TestAzureDocumentDbEventStoreModule<TAuthenticationToken> : NinjectModule
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
			RegisterAzureHelpers();
		}

		#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
			Bind<IEventBuilder<TAuthenticationToken>>()
				.To<AzureDocumentDbEventBuilder<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<IEventDeserialiser<TAuthenticationToken>>()
				.To<AzureDocumentDbEventDeserialiser<TAuthenticationToken>>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices()
		{
		}

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
		/// Register the all Cqrs command handlers
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
			Bind<IAzureDocumentDbEventStoreConnectionStringFactory>()
				.To<TestAzureDocumentDbEventStoreConnectionStringFactory>()
				.InSingletonScope();

			Bind<IEventStore<TAuthenticationToken>>()
				.To<AzureDocumentDbEventStore<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}