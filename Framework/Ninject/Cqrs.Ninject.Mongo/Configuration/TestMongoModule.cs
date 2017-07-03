#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Mongo.Factories;

namespace Cqrs.Ninject.Mongo.Configuration
{
	/// <summary>
	/// A <see cref="MongoModule"/> that assumes the connection string is keyed "MongoDb-Test".
	/// This will generated a random suffix to the database name so as to create a unique store for testing against.
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