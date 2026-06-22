#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Chinchilla.Logging;
using Cqrs.Azure.Storage.DataStores;
using Cqrs.DataStores;

namespace Cqrs.Azure.Storage.Factories
{
	/// <summary>
	/// A factory for obtaining <see cref="IDataStore{TData}"/> collections from Azure Blob Storage.
	/// </summary>
	public abstract class BlobStorageDataStoreFactory
	{
		/// <summary>
		/// Gets the <see cref="IBlobStorageDataStoreConnectionStringFactory"/>.
		/// </summary>
		protected IBlobStorageDataStoreConnectionStringFactory BlobStorageDataStoreConnectionStringFactory { get; private set; }

		/// <summary>
		/// Gets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates and Initialises a new instance of the <see cref="BlobStorageDataStoreFactory"/> class.
		/// </summary>
		protected BlobStorageDataStoreFactory(ILogger logger, IBlobStorageDataStoreConnectionStringFactory blobStorageDataStoreConnectionStringFactory)
		{
			Logger = logger;
			BlobStorageDataStoreConnectionStringFactory = blobStorageDataStoreConnectionStringFactory;
		}
	}
}