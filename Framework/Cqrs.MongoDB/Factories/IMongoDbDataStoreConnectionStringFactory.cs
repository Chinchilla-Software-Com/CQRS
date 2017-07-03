#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.MongoDB.Factories
{
	public interface IMongoDbDataStoreConnectionStringFactory
	{
		string GetDataStoreConnectionString();

		string GetDataStoreDatabaseName();
	}
}