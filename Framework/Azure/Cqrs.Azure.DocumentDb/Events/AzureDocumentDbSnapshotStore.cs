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
using Cqrs.Events;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Snapshots;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb.Events
{
	/// <summary>
	/// A DocumentDb based <see cref="SnapshotStore"/>.
	/// </summary>
	public class AzureDocumentDbSnapshotStore
		: SnapshotStore
	{
		/// <summary>
		/// The properties that must be unique.
		/// </summary>
		protected readonly string[] UniqueIndexProperties = {"AggregateId", "Version"};

		/// <summary>
		/// Gets or sets the <see cref="IAzureDocumentDbSnapshotStoreConnectionStringFactory"/>
		/// </summary>
		protected IAzureDocumentDbSnapshotStoreConnectionStringFactory AzureDocumentDbSnapshotStoreConnectionStringFactory { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAzureDocumentDbHelper"/>
		/// </summary>
		protected IAzureDocumentDbHelper AzureDocumentDbHelper { get; private set; }

		/// <summary>
		/// Instantiate a new instance of <see cref="AzureDocumentDbSnapshotStore"/>.
		/// </summary>
		public AzureDocumentDbSnapshotStore(IConfigurationManager configurationManager, ISnapshotDeserialiser eventDeserialiser, ILogger logger, ICorrelationIdHelper correlationIdHelper, ISnapshotBuilder snapshotBuilder, IAzureDocumentDbHelper azureDocumentDbHelper, IAzureDocumentDbSnapshotStoreConnectionStringFactory azureDocumentDbSnapshotStoreConnectionStringFactory)
			: base(configurationManager, eventDeserialiser, snapshotBuilder, logger, correlationIdHelper)
		{
			AzureDocumentDbHelper = azureDocumentDbHelper;
			AzureDocumentDbSnapshotStoreConnectionStringFactory = azureDocumentDbSnapshotStoreConnectionStringFactory;
		}

		#region Overrides of SnapshotStore

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		protected override Snapshot Get(Type aggregateRootType, string streamName)
		{
			return GetAsync(aggregateRootType, streamName).Result;
		}

		/// <summary>
		/// Saves the provided <paramref name="snapshot"/> into storage.
		/// </summary>
		/// <param name="snapshot">the <see cref="Snapshot"/> to save and store.</param>
		public override void Save(Snapshot snapshot)
		{
			Logger.LogDebug("Persisting aggregate root snapshot", string.Format("{0}\\Save", GetType().Name));
			try
			{
				using (DocumentClient client = AzureDocumentDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreConnectionClient())
				{
					Database database = AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreConnectionDatabaseName()).Result;
					string collectionName = AzureDocumentDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreConnectionCollectionName();
					DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName, UniqueIndexProperties).Result;

					Logger.LogDebug("Creating document for snapshot asynchronously", string.Format("{0}\\Save", GetType().Name));
					AzureDocumentDbHelper.ExecuteFaultTollerantFunction
					(
						() =>
						{
							Task<ResourceResponse<Document>> work = client.CreateDocumentAsync
							(
								collection.SelfLink,
								BuildEventData(snapshot)
							);
							work.ConfigureAwait(false);
							work.Wait();
						}
					);
				}
			}
			finally
			{
				Logger.LogDebug("Persisting aggregate root snapshot... Done", string.Format("{0}\\Save", GetType().Name));
			}
		}

		#endregion

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		protected virtual async Task<Snapshot> GetAsync(Type aggregateRootType, string streamName)
		{
			using (DocumentClient client = AzureDocumentDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreConnectionClient())
			{
				Database database = AzureDocumentDbHelper.CreateOrReadDatabase(client, AzureDocumentDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreConnectionDatabaseName()).Result;
				string collectionName = AzureDocumentDbSnapshotStoreConnectionStringFactory.GetSnapshotStoreConnectionCollectionName();
				DocumentCollection collection = AzureDocumentDbHelper.CreateOrReadCollection(client, database, collectionName, UniqueIndexProperties).Result;

				IOrderedQueryable<EventData> query = client.CreateDocumentQuery<EventData>(collection.SelfLink);

				IEnumerable<EventData> results = query.Where(snapshot => snapshot.AggregateId == streamName);

				return await Task.FromResult(
					AzureDocumentDbHelper.ExecuteFaultTollerantFunction(() =>
						results
							.ToList()
							.OrderByDescending(eventData => eventData.Version)
							.Take(1)
							.Select(EventDeserialiser.Deserialise)
							.SingleOrDefault()
					)
				);
			}
		}
	}
}