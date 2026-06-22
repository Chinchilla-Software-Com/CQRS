#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Chinchilla.Logging;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.MongoDB.Factories;
using MongoDB.Driver;

namespace Cqrs.MongoDB.Tests.Integration
{
	/// <summary>
	/// A <see cref="MongoDbDataStoreFactory"/>
	/// that provides a <see cref="IMongoCollection{TDocument}"/> or <see cref="TestEvent"/>.
	/// </summary>
	public class TestMongoDbDataStoreFactory : MongoDbDataStoreFactory
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="TestMongoDbDataStoreFactory"/>
		/// </summary>
		public TestMongoDbDataStoreFactory(ILogger logger, IMongoDbDataStoreConnectionStringFactory mongoDbDataStoreConnectionStringFactory)
			: base(logger, mongoDbDataStoreConnectionStringFactory)
		{
		}

		/// <summary>
		/// Get a <see cref="IMongoCollection{TestEvent}"/> of <see cref="TestEvent"/>
		/// </summary>
		public IMongoCollection<TestEvent> GetTestEventCollection()
		{
			return GetCollection<TestEvent>();
		}
	}
}