#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.MongoDB.Events;
using Cqrs.Ninject.MongoDB.Configuration;

namespace Cqrs.MongoDB.Tests.Integration.Configuration
{
	/// <summary />
	public class TestMongoDbModule<TAuthenticationToken>
		: MongoDbEventStoreModule<TAuthenticationToken>
	{
		#region Overrides of MongoDbEventStoreModule<TAuthenticationToken>

		/// <summary>
		/// Register the all factories
		/// </summary>
		public override void RegisterFactories()
		{
			Bind<IMongoDbEventStoreConnectionStringFactory>()
				.To<TestMongoEventStoreConnectionStringFactory>()
				.InSingletonScope();
			Bind<IMongoDbSnapshotStoreConnectionStringFactory>()
				.To<TestMongoDbSnapshotStoreConnectionStringFactory>()
				.InSingletonScope();
		}

		#endregion
	}
}