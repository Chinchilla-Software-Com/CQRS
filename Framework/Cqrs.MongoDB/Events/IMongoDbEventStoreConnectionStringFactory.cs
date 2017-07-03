#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.MongoDB.Events
{
	public interface IMongoDbEventStoreConnectionStringFactory
	{
		string GetEventStoreConnectionString();

		string GetEventStoreDatabaseName();
	}
}