#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.MongoDB.Events;
using Cqrs.MongoDB.Factories;
using Ninject.Modules;

namespace Cqrs.Ninject.MongoDB.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="TestMongoDbDataStoreConnectionStringFactory"/> as the
	/// <see cref="IMongoDbEventStoreConnectionStringFactory"/> and <see cref="IMongoDbDataStoreConnectionStringFactory"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class TestMongoDbDataStoreModule<TAuthenticationToken>
		: MongoDbEventStoreModule<TAuthenticationToken>
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