#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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