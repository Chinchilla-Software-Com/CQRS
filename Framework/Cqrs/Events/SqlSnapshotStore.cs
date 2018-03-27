#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Data.Linq;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Exceptions;
using Cqrs.Snapshots;

namespace Cqrs.Events
{
	/// <summary>
	/// Stores the most recent <see cref="Snapshot">snapshots</see> for replay and <see cref="IAggregateRoot{TAuthenticationToken}"/> rehydration on a <see cref="SnapshotAggregateRoot{TAuthenticationToken,TSnapshot}"/> in SqlServer that uses LinqToSql and follows a rigid schema.
	/// </summary>
	public class SqlSnapshotStore
		: ISnapshotStore
	{
		internal const string SqlSnapshotStoreConnectionNameApplicationKey = @"Cqrs.SqlSnapshotStore.ConnectionStringName";

		internal const string SqlSnapshotStoreTableNameApplicationKeyPattern = @"Cqrs.SqlSnapshotStore.CustomTableNames.{0}";

		/// <summary>
		/// The pattern used to generate the stream name.
		/// </summary>
		protected const string CqrsSnapshotStoreStreamNamePattern = "{0}/{1}";

		/// <summary>
		/// Instantiate a new instance of the <see cref="SqlSnapshotStore"/> class.
		/// </summary>
		public SqlSnapshotStore(IConfigurationManager configurationManager, ISnapshotDeserialiser eventDeserialiser, ILogger logger, ICorrelationIdHelper correlationIdHelper, ISnapshotBuilder snapshotBuilder)
		{
			ConfigurationManager = configurationManager;
			EventDeserialiser = eventDeserialiser;
			Logger = logger;
			CorrelationIdHelper = correlationIdHelper;
			SnapshotBuilder = snapshotBuilder;
		}

		/// <summary>
		/// The <see cref="ISnapshotDeserialiser"/> used to deserialise snapshots.
		/// </summary>
		protected ISnapshotDeserialiser EventDeserialiser { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// The <see cref="ILogger"/> to use.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// The <see cref="ICorrelationIdHelper"/> to use.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// The <see cref="ISnapshotBuilder"/> used to build snapshots.
		/// </summary>
		protected ISnapshotBuilder SnapshotBuilder { get; private set; }

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
			while (aggregateRootType != null && !aggregateRootType.IsGenericType && aggregateRootType.GetGenericArguments().Length != 2)
				aggregateRootType = aggregateRootType.BaseType;
			if (aggregateRootType == null)
				return null;

			aggregateRootType = aggregateRootType.GetGenericArguments()[1];
			if (aggregateRootType.BaseType != typeof(Snapshot))
				return null;

			string streamName = string.Format(CqrsSnapshotStoreStreamNamePattern, aggregateRootType.FullName, id);

			using (DataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
			{
				EventData query = GetEventStoreSnapshotTable(dbDataContext)
					.AsQueryable()
					.Where(snapshot => snapshot.AggregateId == streamName)
					.OrderByDescending(eventData => eventData.Version)
					.Take(1)
					.SingleOrDefault();

				return EventDeserialiser.Deserialise(query);
			}
		}

		/// <summary>
		/// Saves the provided <paramref name="snapshot"/> into storage.
		/// </summary>
		/// <param name="snapshot">the <see cref="Snapshot"/> to save and store.</param>
		public virtual void Save(Snapshot snapshot)
		{
			using (DataContext dbDataContext = CreateDbDataContext(snapshot.GetType().Name))
			{
				Add(dbDataContext, snapshot);
			}
		}

		#endregion

		/// <summary>
		/// Creates a new <see cref="DataContext"/> using connection string settings from <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual DataContext CreateDbDataContext(string aggregateRootTypeName = null)
		{
			string connectionStringKey;
			string applicationKey;
			if (!ConfigurationManager.TryGetSetting(SqlSnapshotStoreConnectionNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
				throw new MissingApplicationSettingForConnectionStringException(SqlSnapshotStoreConnectionNameApplicationKey);
			try
			{
				connectionStringKey = System.Configuration.ConfigurationManager.ConnectionStrings[applicationKey].ConnectionString;
			}
			catch (NullReferenceException exception)
			{
				throw new MissingConnectionStringException(applicationKey, exception);
			}

			string tableName;
			if (!string.IsNullOrWhiteSpace(aggregateRootTypeName) && ConfigurationManager.TryGetSetting(string.Format(SqlSnapshotStoreTableNameApplicationKeyPattern, aggregateRootTypeName), out tableName) && !string.IsNullOrEmpty(tableName))
			{
				bool autoname;
				if (bool.TryParse(tableName, out autoname))
				{
					if (autoname)
						return SqlEventStoreDataContext.New<EventData>(aggregateRootTypeName.Replace(".", "_"), connectionStringKey);
				}
				else
					return SqlEventStoreDataContext.New<EventData>(tableName, connectionStringKey);
			}

			return SqlEventStoreDataContext.New<EventData>("Snapshots", connectionStringKey);
		}

		/// <summary>
		/// Gets the <see cref="Table{TEntity}"/> of <see cref="Snapshot"/>.
		/// </summary>
		/// <param name="dbDataContext">The <see cref="DataContext"/> to use.</param>
		protected virtual Table<EventData> GetEventStoreSnapshotTable(DataContext dbDataContext)
		{
			// Get a typed table to run queries.
			return dbDataContext.GetTable<EventData>();
		}

		/// <summary>
		/// Persist the provided <paramref name="data"/> into SQL Server using the provided <paramref name="dbDataContext"/>.
		/// </summary>
		protected virtual void Add(DataContext dbDataContext, Snapshot data)
		{
			Logger.LogDebug("Adding data to the SQL snapshot database", "SqlSnapshotStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				EventData eventData = SnapshotBuilder.CreateFrameworkEvent(data);
				string streamName = GenerateStreamName(data.GetType(), data.Id);
				eventData.AggregateId = streamName;
				eventData.AggregateRsn = data.Id;
				eventData.Version = data.Version;
				eventData.CorrelationId = CorrelationIdHelper.GetCorrelationId();
				GetEventStoreSnapshotTable(dbDataContext).InsertOnSubmit(eventData);
				dbDataContext.SubmitChanges();
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the SQL snapshot database took {0}.", end - start), "SqlSnapshotStore\\Add");
			}
			catch (Exception exception)
			{
				Logger.LogError("There was an issue persisting data to the SQL snapshot database.", exception: exception);
				throw;
			}
			finally
			{
				Logger.LogDebug("Adding data to the SQL snapshot database... Done", "SqlSnapshotStore\\Add");
			}
		}

		/// <summary>
		/// Generate a unique stream name based on the provided <paramref name="aggregateRootType"/> and the <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="aggregateId">The ID of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		protected virtual string GenerateStreamName(Type aggregateRootType, Guid aggregateId)
		{
			return string.Format(CqrsSnapshotStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);
		}
	}
}