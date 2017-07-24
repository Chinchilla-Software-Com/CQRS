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
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="TestMongoDataStoreConnectionStringFactory"/> as the
	/// <see cref="IMongoDataStoreConnectionStringFactory"/>.
	/// </summary>
	public class TestMongoModule : MongoModule
	{
		/// <summary>
		/// Register the all factories
		/// </summary>
		public override void RegisterFactories()
		{
			Bind<IMongoDataStoreConnectionStringFactory>()
				.To<TestMongoDataStoreConnectionStringFactory>()
				.InSingletonScope();
		}
	}
}