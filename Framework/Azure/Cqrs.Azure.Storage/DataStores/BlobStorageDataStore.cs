#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Azure.Storage.DataStores
{
	/// <summary>
	/// A <see cref="IDataStore{TData}"/> that uses Azure Blob Storage for storage.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of <see cref="IEntity"/> the <see cref="IDataStore{TData}"/> will contain.</typeparam>
	public class BlobStorageDataStore<TData>
		: BlobStorageStore<TData>
		, IDataStore<TData>
		where TData : Entity
	{
		internal Func<string> GenerateFolderName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageDataStore{TData}"/> class using the specified container.
		/// </summary>
		public BlobStorageDataStore(ILogger logger, IBlobStorageDataStoreConnectionStringFactory blobStorageDataStoreConnectionStringFactory)
			: base(logger)
		{
			GetContainerName = blobStorageDataStoreConnectionStringFactory.GetBaseContainerName;
			IsContainerPublic = blobStorageDataStoreConnectionStringFactory.IsContainerPublic<TData>;
			GenerateFolderName = blobStorageDataStoreConnectionStringFactory.GetEntityName<TData>;
			GenerateFileName = data => string.Format("{0}\\{1}", GenerateFolderName(), data.Rsn.ToString("N"));

#if NET472
			Initialise(blobStorageDataStoreConnectionStringFactory);
#else
			Task.Run(async () => {
				await InitialiseAsync(blobStorageDataStoreConnectionStringFactory);
			}).Wait();
#endif
		}

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		public virtual
#if NET472
			void Remove
#else
			async Task RemoveAsync
#endif
				(TData data)
		{
			data.IsDeleted = true;
#if NET472
			Update
#else
			await UpdateAsync
#endif
				(data);
		}

		#endregion

		/// <summary>
		/// Get all <typeparamref name="TData"/> items in the folder.
		/// </summary>
		public virtual
#if NET472
			IEnumerable<TData> GetByFolder
#else
			async Task<IEnumerable<TData>> GetByFolderAsync
#endif
				()
		{
			string folderName = GenerateFolderName();
			return
			(
#if NET472
				OpenStreamsForReading
#else
				await OpenStreamsForReadingAsync
#endif
					(folderName: folderName)
			)
			.Select(Deserialise);
		}
	}
}