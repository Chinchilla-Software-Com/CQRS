#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Domain;
using Cqrs.Entities;
using Cqrs.Exceptions;
using Cqrs.Messages;

namespace Cqrs.Events
{
	/// <summary>
	/// A simplified SqlServer based <see cref="EventStore{TAuthenticationToken}"/> that uses LinqToSql and follows a rigid schema.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class SqlEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken> 
	{
		internal const string SqlEventStoreDbFileOrServerOrConnectionApplicationKey = @"SqlEventStoreDbFileOrServerOrConnection";

		internal const string SqlEventStoreConnectionNameApplicationKey = @"Cqrs.SqlEventStore.ConnectionStringName";

		internal const string OldSqlEventStoreGetByCorrelationIdCommandTimeout = @"SqlEventStoreGetByCorrelationIdCommandTimeout";

		internal const string SqlEventStoreGetByCorrelationIdCommandTimeout = @"Cqrs.SqlEventStore.GetByCorrelationId.CommandTimeout";

		internal const string SqlEventStoreTableNameApplicationKeyPattern = @"Cqrs.SqlEventStore.CustomTableNames.{0}";

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Instantiate a new instance of the <see cref="SqlEventStore{TAuthenticationToken}"/> class.
		/// </summary>
		public SqlEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IConfigurationManager configurationManager)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			ConfigurationManager = configurationManager;
		}

		#region Overrides of EventStore<TAuthenticationToken>

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			using (DataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
			{
				IEnumerable<EventData> query = GetEventStoreTable(dbDataContext)
					.AsQueryable()
					.Where(eventData => eventData.AggregateId == streamName && eventData.Version > fromVersion)
					.OrderByDescending(eventData => eventData.Version);

				if (useLastEventOnly)
					query = query.AsQueryable().Take(1);

				return query
					.Select(EventDeserialiser.Deserialise)
					.ToList();
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetToVersion(Type aggregateRootType, Guid aggregateId, int version)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			using (DataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
			{
				IEnumerable<EventData> query = GetEventStoreTable(dbDataContext)
					.AsQueryable()
					.Where(eventData => eventData.AggregateId == streamName && eventData.Version <= version)
					.OrderByDescending(eventData => eventData.Version);

				return query
					.Select(EventDeserialiser.Deserialise)
					.ToList();
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetToDate(Type aggregateRootType, Guid aggregateId, DateTime versionedDate)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			using (DataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
			{
				IEnumerable<EventData> query = GetEventStoreTable(dbDataContext)
					.AsQueryable()
					.Where(eventData => eventData.AggregateId == streamName && eventData.Timestamp <= versionedDate)
					.OrderByDescending(eventData => eventData.Version);

				return query
					.Select(EventDeserialiser.Deserialise)
					.ToList();
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/> from and including the provided <paramref name="fromVersionedDate"/> up to and including the provided <paramref name="toVersionedDate"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersionedDate">Load events from and including from this <see cref="DateTime"/></param>
		/// <param name="toVersionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public override IEnumerable<IEvent<TAuthenticationToken>> GetBetweenDates(Type aggregateRootType, Guid aggregateId, DateTime fromVersionedDate, DateTime toVersionedDate)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			using (DataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
			{
				IEnumerable<EventData> query = GetEventStoreTable(dbDataContext)
					.AsQueryable()
					.Where(eventData => eventData.AggregateId == streamName && eventData.Timestamp >= fromVersionedDate && eventData.Timestamp <= toVersionedDate)
					.OrderByDescending(eventData => eventData.Version);

				return query
					.Select(EventDeserialiser.Deserialise)
					.ToList();
			}
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public override IEnumerable<EventData> Get(Guid correlationId)
		{
			using (DataContext dbDataContext = CreateDbDataContext())
			{
				string commandTimeoutValue;
				int commandTimeout;
				bool found = ConfigurationManager.TryGetSetting(SqlEventStoreGetByCorrelationIdCommandTimeout, out commandTimeoutValue);
				if (!found)
					found = ConfigurationManager.TryGetSetting(OldSqlEventStoreGetByCorrelationIdCommandTimeout, out commandTimeoutValue);
				if (found && int.TryParse(commandTimeoutValue, out commandTimeout))
					dbDataContext.CommandTimeout = commandTimeout;

				IEnumerable<EventData> query = GetEventStoreTable(dbDataContext)
					.AsQueryable()
					.Where(eventData => eventData.CorrelationId == correlationId)
					.OrderBy(eventData => eventData.Timestamp);

				return query.ToList();
			}
		}

		/// <summary>
		/// Persist the provided <paramref name="eventData"/> into SQL Server.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to persist.</param>
		protected override void PersistEvent(EventData eventData)
		{
			using (DataContext dbDataContext = CreateDbDataContext(eventData.AggregateId.Substring(0, eventData.AggregateId.IndexOf("/", StringComparison.InvariantCultureIgnoreCase))))
			{
				Add(dbDataContext, eventData);
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
			if (!ConfigurationManager.TryGetSetting(SqlEventStoreConnectionNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
			{
				if (!ConfigurationManager.TryGetSetting(SqlEventStoreDbFileOrServerOrConnectionApplicationKey, out connectionStringKey) || string.IsNullOrEmpty(connectionStringKey))
				{
					if (!ConfigurationManager.TryGetSetting(SqlDataStore<Entity>.SqlDataStoreDbFileOrServerOrConnectionApplicationKey, out connectionStringKey) || string.IsNullOrEmpty(connectionStringKey))
					{
						throw new MissingApplicationSettingForConnectionStringException(SqlEventStoreConnectionNameApplicationKey);
					}
				}
			}
			else
			{
				ConnectionStringSettings connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[applicationKey];
				if (connectionString == null)
					throw new MissingConnectionStringException(applicationKey);
				connectionStringKey = connectionString.ConnectionString;
			}

			string tableName;
			if (!string.IsNullOrWhiteSpace(aggregateRootTypeName) && ConfigurationManager.TryGetSetting(string.Format(SqlEventStoreTableNameApplicationKeyPattern, aggregateRootTypeName), out tableName) && !string.IsNullOrEmpty(tableName))
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

			return new SqlEventStoreDataContext(connectionStringKey);
		}

		/// <summary>
		/// Gets the <see cref="Table{TEntity}"/> of <see cref="EventData"/>.
		/// </summary>
		/// <param name="dbDataContext">The <see cref="DataContext"/> to use.</param>
		protected virtual Table<EventData> GetEventStoreTable(DataContext dbDataContext)
		{
			// Get a typed table to run queries.
			return dbDataContext.GetTable<EventData>();
		}

		/// <summary>
		/// Persist the provided <paramref name="data"/> into SQL Server using the provided <paramref name="dbDataContext"/>.
		/// </summary>
		protected virtual void Add(DataContext dbDataContext, EventData data)
		{
			Logger.LogDebug("Adding data to the SQL eventstore database", "SqlEventStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				GetEventStoreTable(dbDataContext).InsertOnSubmit(data);
				dbDataContext.SubmitChanges();
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the SQL eventstore database took {0}.", end - start), "SqlEventStore\\Add");
			}
			catch (Exception exception)
			{
				Logger.LogError("There was an issue persisting data to the SQL event store.", exception: exception);
				throw;
			}
			finally
			{
				Logger.LogDebug("Adding data to the SQL eventstore database... Done", "SqlEventStore\\Add");
			}
		}
	}
}