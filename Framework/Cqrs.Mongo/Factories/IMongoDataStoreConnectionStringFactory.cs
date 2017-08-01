#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Mongo.Factories
{
	/// <summary>
	/// A factory for MongoDb related connection string settings.
	/// </summary>
	public interface IMongoDataStoreConnectionStringFactory
	{
		/// <summary>
		/// Get the connection string for the MongoDB server.
		/// </summary>
		string GetMongoConnectionString();

		/// <summary>
		/// Get the name of database on the MongoDB server.
		/// </summary>
		string GetMongoDatabaseName();
	}
}