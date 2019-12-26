#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
#if NET40
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#else
using Microsoft.EntityFrameworkCore;
#endif
using System.Linq;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Messages;

namespace Cqrs.Events
{
	/// <summary>
	/// A simplified SqlServer based <see cref="EventStore{TAuthenticationToken}"/> that uses Entity Framework and follows a rigid schema.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class SqlEventStore<TAuthenticationToken>
		: EventStore<TAuthenticationToken> 
	{
		internal const string SqlEventStoreConnectionNameApplicationKey = @"Cqrs.SqlEventStore.ConnectionStringName";

		internal const string SqlEventStoreGetByCorrelationIdCommandTimeout = @"Cqrs.SqlEventStore.GetByCorrelationId.CommandTimeout";

		internal const string SqlEventStoreTableNameApplicationKeyPattern = @"Cqrs.SqlEventStore.CustomTableNames.{0}";

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Instantiate a new instance of the <see cref="LinqToSqlEventStore{TAuthenticationToken}"/> class.
		/// </summary>
		public LinqToSqlEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IConfigurationManager configurationManager)
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

			using (SqlEventStoreDataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
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

			using (SqlEventStoreDataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
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

			using (SqlEventStoreDataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
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

			using (SqlEventStoreDataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
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
			using (SqlEventStoreDataContext dbDataContext = CreateDbDataContext())
			{
				string commandTimeoutValue;
				int commandTimeout;
				bool found = ConfigurationManager.TryGetSetting(SqlEventStoreGetByCorrelationIdCommandTimeout, out commandTimeoutValue);
				if (found && int.TryParse(commandTimeoutValue, out commandTimeout))
				{
#if NET40
					// Get the ObjectContext related to this DbContext
					var objectContext = (dbDataContext as IObjectContextAdapter).ObjectContext;

					// Sets the command timeout for all the commands
					objectContext.CommandTimeout = commandTimeout;
#else
					dbDataContext.Database.SetCommandTimeout(commandTimeout);
#endif
				}

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
			using (SqlEventStoreDataContext dbDataContext = CreateDbDataContext(eventData.AggregateId.Substring(0, eventData.AggregateId.IndexOf("/", StringComparison.InvariantCultureIgnoreCase))))
			{
				Add(dbDataContext, eventData);
			}
		}

#endregion

		/// <summary>
		/// Creates a new <see cref="DbContext"/> using connection string settings from <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual SqlEventStoreDataContext CreateDbDataContext(string aggregateRootTypeName = null)
		{
			string connectionStringKey = ConfigurationManager.GetConnectionStringBySettingKey(SqlEventStoreConnectionNameApplicationKey, true, true);

			string tableName;
			if (!string.IsNullOrWhiteSpace(aggregateRootTypeName) && ConfigurationManager.TryGetSetting(string.Format(SqlEventStoreTableNameApplicationKeyPattern, aggregateRootTypeName), out tableName) && !string.IsNullOrEmpty(tableName))
			{
				bool autoname;
				if (bool.TryParse(tableName, out autoname))
				{
					if (autoname)
						return SqlEventStoreDataContext.New(aggregateRootTypeName.Replace(".", "_"), connectionStringKey);
				}
				else
					return SqlEventStoreDataContext.New(tableName, connectionStringKey);
			}

			return new SqlEventStoreDataContext(connectionStringKey);
		}

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of <see cref="EventData"/>.
		/// </summary>
		/// <param name="dbDataContext">The <see cref="SqlEventStoreDataContext"/> to use.</param>
		protected virtual DbSet<EventData> GetEventStoreTable(SqlEventStoreDataContext dbDataContext)
		{
			// Get a typed table to run queries.
			return dbDataContext.Set<EventData>();
		}

		/// <summary>
		/// Persist the provided <paramref name="data"/> into SQL Server using the provided <paramref name="dbDataContext"/>.
		/// </summary>
		protected virtual void Add(SqlEventStoreDataContext dbDataContext, EventData data)
		{
			Logger.LogDebug("Adding data to the SQL eventstore database", "SqlEventStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				GetEventStoreTable(dbDataContext).Add(data);
				dbDataContext.SaveChanges();
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