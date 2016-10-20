#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.MongoDB.Events;
using Cqrs.MongoDB.Factories;

namespace Cqrs.Ninject.MongoDB.Configuration
{
	/// <summary>
	/// A <see cref="MongoDbDataStoreModule"/> that assumes the connection string is keyed "MongoDb-Test".
	/// This will generated a random suffix to the database name so as to create a unique store for testing against.
	/// </summary>
	public class TestMongoDbDataStoreModule<TAuthenticationToken> : MongoDbEventStoreModule<TAuthenticationToken>
	{
		/// <summary>
		/// Register the all factories
		/// </summary>
		public override void RegisterFactories()
		{
			Bind<IMongoDbEventStoreConnectionStringFactory>()
				.To<TestMongoDbDataStoreConnectionStringFactory>()
				.InSingletonScope();
			Bind<IMongoDbDataStoreConnectionStringFactory>()
				.To<TestMongoDbDataStoreConnectionStringFactory>()
				.InSingletonScope();
		}
	}
}