#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Mongo.Factories;
using Ninject.Modules;

namespace Cqrs.Ninject.Mongo.Configuration
{
	public class MongoModule : NinjectModule
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
			Bind<IMongoDataStoreConnectionStringFactory>()
				.To<MongoDataStoreConnectionStringFactory>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the all Cqrs command handlers
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
		}
	}
}