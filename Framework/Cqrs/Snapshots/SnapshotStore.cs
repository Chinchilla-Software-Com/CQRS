#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Snapshots
{
	/// <summary>
	/// Stores the most recent <see cref="Snapshot">snapshots</see> for replay and <see cref="IAggregateRoot{TAuthenticationToken}"/> rehydration on a <see cref="SnapshotAggregateRoot{TAuthenticationToken,TSnapshot}"/>.
	/// </summary>
	public abstract class SnapshotStore
		: ISnapshotStore
	{
		/// <summary>
		/// Instantiate a new instance of the <see cref="SnapshotStore"/> class.
		/// </summary>
		protected SnapshotStore(IConfigurationManager configurationManager, ISnapshotDeserialiser eventDeserialiser, ISnapshotBuilder snapshotBuilder, ILogger logger, ICorrelationIdHelper correlationIdHelper)
		{
			ConfigurationManager = configurationManager;
			EventDeserialiser = eventDeserialiser;
			SnapshotBuilder = snapshotBuilder;
			Logger = logger;
			CorrelationIdHelper = correlationIdHelper;
		}

		/// <summary>
		/// The pattern used to generate the stream name.
		/// </summary>
		protected const string CqrsSnapshotStoreStreamNamePattern = "{0}/{1}";

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// The <see cref="ISnapshotDeserialiser"/> used to deserialise snapshots.
		/// </summary>
		protected ISnapshotDeserialiser EventDeserialiser { get; private set; }

		/// <summary>
		/// The <see cref="ISnapshotBuilder"/> used to build snapshots.
		/// </summary>
		protected ISnapshotBuilder SnapshotBuilder { get; private set; }

		/// <summary>
		/// The <see cref="ILogger"/> to use.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// The <see cref="ICorrelationIdHelper"/> to use.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		#region Implementation of ISnapshotStore

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> to find a snapshot for.</typeparam>
		/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to get the most recent <see cref="Snapshot"/> of.</param>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		public virtual Snapshot Get<TAggregateRoot>(Guid id)
		{
			Type aggregateRootType = typeof (TAggregateRoot);
			return Get(aggregateRootType, id);
		}

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <param name="aggregateRootType">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> to find a snapshot for.</param>
		/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to get the most recent <see cref="Snapshot"/> of.</param>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		public Snapshot Get(Type aggregateRootType, Guid id)
		{
			while (aggregateRootType != null && !aggregateRootType.IsGenericType && aggregateRootType.GetGenericArguments().Length != 2)
				aggregateRootType = aggregateRootType.BaseType;
			if (aggregateRootType == null)
				return null;

			aggregateRootType = aggregateRootType.GetGenericArguments()[1];
			if (aggregateRootType.BaseType != typeof(Snapshot))
				return null;

			string streamName = string.Format(CqrsSnapshotStoreStreamNamePattern, aggregateRootType.FullName, id);

			return Get(aggregateRootType, streamName);
		}

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		protected abstract Snapshot Get(Type aggregateRootType, string streamName);

		/// <summary>
		/// Saves the provided <paramref name="snapshot"/> into storage.
		/// </summary>
		/// <param name="snapshot">the <see cref="Snapshot"/> to save and store.</param>
		public abstract void Save(Snapshot snapshot);

		#endregion

		/// <summary>
		/// Generate a unique stream name based on the provided <paramref name="aggregateRootType"/> and the <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="aggregateId">The ID of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		protected virtual string GenerateStreamName(Type aggregateRootType, Guid aggregateId)
		{
			return string.Format(CqrsSnapshotStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);
		}

		/// <summary>
		/// Builds the <see cref="EventData"/> from the <paramref name="snapshot"/>.
		/// </summary>
		protected virtual EventData BuildEventData(Snapshot snapshot)
		{
			Logger.LogDebug("Building the snapshot event data", "SnapshotStore\\BuildEventData");
			try
			{
				DateTime start = DateTime.Now;
				EventData eventData = SnapshotBuilder.CreateFrameworkEvent(snapshot);
				string streamName = GenerateStreamName(snapshot.GetType(), snapshot.Id);
				eventData.AggregateId = streamName;
				eventData.AggregateRsn = snapshot.Id;
				eventData.Version = snapshot.Version;
				eventData.CorrelationId = CorrelationIdHelper.GetCorrelationId();
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Building the snapshot event data took {0}.", end - start), "SnapshotStore\\BuildEventData");
				return eventData;
			}
			catch (Exception exception)
			{
				Logger.LogError("There was an issue building the snapshot event data.", exception: exception);
				throw;
			}
			finally
			{
				Logger.LogDebug("Building the snapshot event data... Done", "SnapshotStore\\BuildEventData");
			}
		}
	}
}