#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Entities;

namespace Cqrs.Events
{
	/// <summary>
	/// A simplified SqlServer based <see cref="EventStore{TAuthenticationToken}"/> that uses LinqToSql and follows a rigid schema.
	/// </summary>
	public class SqlEventStore<TAuthenticationToken> : EventStore<TAuthenticationToken> 
	{
		internal const string SqlEventStoreDbFileOrServerOrConnectionApplicationKey = @"SqlEventStoreDbFileOrServerOrConnection";

		internal const string SqlEventStoreConnectionNameApplicationKey = @"Cqrs.SqlEventStore.ConnectionStringName";

		internal const string OldSqlEventStoreGetByCorrelationIdCommandTimeout = @"SqlEventStoreGetByCorrelationIdCommandTimeout";

		internal const string SqlEventStoreGetByCorrelationIdCommandTimeout = @"Cqrs.SqlEventStore.GetByCorrelationId.CommandTimeout";

		protected IConfigurationManager ConfigurationManager { get; private set; }

		public SqlEventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger, IConfigurationManager configurationManager)
			: base(eventBuilder, eventDeserialiser, logger)
		{
			ConfigurationManager = configurationManager;
		}

		#region Overrides of EventStore<TAuthenticationToken>

		public override IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);

			using (DataContext dbDataContext = CreateDbDataContext())
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

		protected override void PersistEvent(EventData eventData)
		{
			using (DataContext dbDataContext = CreateDbDataContext())
			{
				Add(dbDataContext, eventData);
			}
		}

		#endregion

		protected virtual DataContext CreateDbDataContext()
		{
			string connectionStringKey;
			string applicationKey;
			if (!ConfigurationManager.TryGetSetting(SqlEventStoreConnectionNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
			{
				if (!ConfigurationManager.TryGetSetting(SqlEventStoreDbFileOrServerOrConnectionApplicationKey, out connectionStringKey) || string.IsNullOrEmpty(connectionStringKey))
				{
					if (!ConfigurationManager.TryGetSetting(SqlDataStore<Entity>.SqlDataStoreDbFileOrServerOrConnectionApplicationKey, out connectionStringKey) || string.IsNullOrEmpty(connectionStringKey))
					{
						throw new NullReferenceException(string.Format("No application setting named '{0}' was found in the configuration file with the name of a connection string to look for.", SqlEventStoreConnectionNameApplicationKey));
					}
				}
			}
			else
			{
				try
				{
					connectionStringKey = System.Configuration.ConfigurationManager.ConnectionStrings[applicationKey].ConnectionString;
				}
				catch (NullReferenceException exception)
				{
					throw new NullReferenceException(string.Format("No connection string setting named '{0}' was found in the configuration file with the SQL Event Store connection string.", applicationKey), exception);
				}
			}
			return new DataContext(connectionStringKey);
		}

		protected virtual Table<EventData> GetEventStoreTable(DataContext dbDataContext)
		{
			// Get a typed table to run queries.
			return dbDataContext.GetTable<EventData>();
		}

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
