#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Events;

namespace Cqrs.MongoDB.Events
{
	/// <summary>
	/// A factory for getting connection strings and database names for <see cref="IEventStore{TAuthenticationToken}"/> access.
	/// </summary>
	public interface IMongoDbEventStoreConnectionStringFactory
	{
		/// <summary>
		/// Gets the current connection string.
		/// </summary>
		string GetEventStoreConnectionString();

		/// <summary>
		/// Gets the current database name.
		/// </summary>
		string GetEventStoreDatabaseName();
	}
}