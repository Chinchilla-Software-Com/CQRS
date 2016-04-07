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
	public class SqlEventStore<TAuthenticationToken> : EventStore<TAuthenticationToken> 
	{
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
			// Use a connection string.
			return new DataContext(ConfigurationManager.GetSetting(SqlDataStore<Entity>.SqlDataStoreDbFileOrServerOrConnectionApplicationKey));
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
