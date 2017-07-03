#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.Logging;
using Cqrs.Azure.BlobStorage.DataStores;

namespace Cqrs.Azure.BlobStorage.Factories
{
	/// <summary>
	/// A factory for obtaining DataStore collections from Azure Blob Storage
	/// </summary>
	public abstract class BlobStorageDataStoreFactory
	{
		protected IBlobStorageDataStoreConnectionStringFactory BlobStorageDataStoreConnectionStringFactory { get; private set; }

		protected ILogger Logger { get; private set; }

		protected BlobStorageDataStoreFactory(ILogger logger, IBlobStorageDataStoreConnectionStringFactory blobStorageDataStoreConnectionStringFactory)
		{
			Logger = logger;
			BlobStorageDataStoreConnectionStringFactory = blobStorageDataStoreConnectionStringFactory;
		}
	}
}