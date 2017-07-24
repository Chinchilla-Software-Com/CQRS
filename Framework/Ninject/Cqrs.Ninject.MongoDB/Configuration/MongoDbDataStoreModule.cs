#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.MongoDB.Factories;
using Ninject.Modules;

namespace Cqrs.Ninject.MongoDB.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="MongoDbDataStoreConnectionStringFactory"/> as the <see cref="IMongoDbDataStoreConnectionStringFactory"/>.
	/// </summary>
	public class MongoDbDataStoreModule : NinjectModule
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
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices()
		{
		}

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
			Bind<IMongoDbDataStoreConnectionStringFactory>()
				.To<MongoDbDataStoreConnectionStringFactory>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register any CQRS requirements
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
		}
	}
}