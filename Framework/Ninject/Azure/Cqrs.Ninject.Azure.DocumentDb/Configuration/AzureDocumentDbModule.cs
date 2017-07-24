#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Linq;
using Cqrs.Azure.DocumentDb;
using Cqrs.Azure.DocumentDb.Factories;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.DocumentDb.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureDocumentDbDataStoreConnectionStringFactory"/> as the <see cref="IAzureDocumentDbDataStoreConnectionStringFactory"/>.
	/// </summary>
	public class AzureDocumentDbModule : NinjectModule
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
			Bind<IAzureDocumentDbDataStoreConnectionStringFactory>()
				.To<AzureDocumentDbDataStoreConnectionStringFactory>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices()
		{
		}

		/// <summary>
		/// Register any CQRS requirements.
		/// </summary>
		public virtual void RegisterCqrsRequirements()
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
	}
}