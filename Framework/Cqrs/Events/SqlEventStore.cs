#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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

		internal const string SqlEventStoreGetByCorrelationIdCommandTimeout = @"SqlEventStoreGetByCorrelationIdCommandTimeout";

		public IConfigurationManager ConfigurationManager { get; set; }

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
				if (ConfigurationManager.TryGetSetting(SqlEventStoreGetByCorrelationIdCommandTimeout, out commandTimeoutValue))
					if (int.TryParse(commandTimeoutValue, out commandTimeout))
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
			if (!ConfigurationManager.TryGetSetting(SqlEventStoreDbFileOrServerOrConnectionApplicationKey, out connectionStringKey) || string.IsNullOrEmpty(connectionStringKey))
				connectionStringKey = ConfigurationManager.GetSetting(SqlDataStore<Entity>.SqlDataStoreDbFileOrServerOrConnectionApplicationKey);
			return new DataContext(connectionStringKey);
		}

		protected virtual Table<EventData> GetEventStoreTable(DataContext dbDataContext)
		{
			// Get a typed table to run queries.
			return dbDataContext.GetTable<EventData>();
		}

		protected virtual void Add(DataContext dbDataContext, EventData data)
		{
			Logger.LogDebug("Adding data to the Sql eventstore database", "SqlEventStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				GetEventStoreTable(dbDataContext).InsertOnSubmit(data);
				dbDataContext.SubmitChanges();
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the Sql eventstore database took {0}.", end - start), "SqlEventStore\\Add");
			}
			finally
			{
				Logger.LogDebug("Adding data to the Sql eventstore database... Done", "SqlEventStore\\Add");
			}
		}

	}
}
