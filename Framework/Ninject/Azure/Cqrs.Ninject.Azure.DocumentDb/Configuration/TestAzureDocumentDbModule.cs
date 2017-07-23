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
using Cqrs.Ninject.Azure.DocumentDb.Factories;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.DocumentDb.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class TestAzureDocumentDbModule : NinjectModule
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
				.To<TestAzureDocumentDbDataStoreConnectionStringFactory>()
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