#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Mongo.Factories
{
	public interface IMongoDataStoreConnectionStringFactory
	{
		string GetMongoConnectionString();

		string GetMongoDatabaseName();
	}
}