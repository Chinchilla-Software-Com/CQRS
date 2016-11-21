#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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