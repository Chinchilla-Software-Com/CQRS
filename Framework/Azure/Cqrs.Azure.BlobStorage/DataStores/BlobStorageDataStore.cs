#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.Logging;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage.DataStores
{
	public class BlobStorageDataStore<TData>
		: BlobStorageStore<TData>
		, IDataStore<TData>
		where TData : Entity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorage"/> class using the specified container.
		/// </summary>
		public BlobStorageDataStore(ILogger logger, IBlobStorageDataStoreConnectionStringFactory blobStorageDataStoreConnectionStringFactory)
			: base(logger)
		{
			GetContainerName = blobStorageDataStoreConnectionStringFactory.GetContainerName<TData>;
			IsContainerPublic = blobStorageDataStoreConnectionStringFactory.IsContainerPublic<TData>;
			GenerateFileName = data => data.Rsn.ToString("N");

			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Initialise(blobStorageDataStoreConnectionStringFactory);
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public void Remove(TData data)
		{
			data.IsLogicallyDeleted = true;
			Add(data);
		}

		#endregion
	}
}