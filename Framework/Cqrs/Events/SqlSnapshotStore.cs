﻿#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Configuration;
#if NET40_OR_GREATER
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#else
using Microsoft.EntityFrameworkCore;
#endif
using System.Linq;
using Chinchilla.Logging;
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
		: SnapshotStore
	{
		internal const string SqlSnapshotStoreConnectionNameApplicationKey = @"Cqrs.SqlSnapshotStore.ConnectionStringName";

		internal const string SqlSnapshotStoreTableNameApplicationKeyPattern = @"Cqrs.SqlSnapshotStore.CustomTableNames.{0}";

		/// <summary>
		/// Instantiate a new instance of the <see cref="SqlSnapshotStore"/> class.
		/// </summary>
		public SqlSnapshotStore(IConfigurationManager configurationManager, ISnapshotDeserialiser eventDeserialiser, ILogger logger, ICorrelationIdHelper correlationIdHelper, ISnapshotBuilder snapshotBuilder)
			: base(configurationManager, eventDeserialiser, snapshotBuilder, logger, correlationIdHelper)
		{
		}

		#region Implementation of ISnapshotStore

		/// <summary>
		/// Get the latest <see cref="Snapshot"/> from storage.
		/// </summary>
		/// <returns>The most recent <see cref="Snapshot"/> of</returns>
		protected override Snapshot Get(Type aggregateRootType, string streamName)
		{
			using (SqlEventStoreDataContext dbDataContext = CreateDbDataContext(aggregateRootType.FullName))
			{
				EventData query = GetEventStoreSnapshotTable(dbDataContext)
					.AsQueryable()
					.Where(snapshot => snapshot.AggregateId == streamName)
					.OrderByDescending(eventData => eventData.Version)
					.Take(1)
					.SingleOrDefault();

				if (query == null)
					return null;
				return EventDeserialiser.Deserialise(query);
			}
		}

		/// <summary>
		/// Saves the provided <paramref name="snapshot"/> into storage.
		/// </summary>
		/// <param name="snapshot">the <see cref="Snapshot"/> to save and store.</param>
		public override void Save(Snapshot snapshot)
		{
			using (SqlEventStoreDataContext dbDataContext = CreateDbDataContext(snapshot.GetType().Name))
				Add(dbDataContext, snapshot);
		}

		#endregion

		/// <summary>
		/// Creates a new <see cref="DbContext"/> using connection string settings from ConfigurationManager.
		/// </summary>
		protected virtual SqlEventStoreDataContext CreateDbDataContext(string aggregateRootTypeName = null)
		{
			string connectionStringKey;
			string applicationKey;
			if (!ConfigurationManager.TryGetSetting(SqlSnapshotStoreConnectionNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
				throw new MissingApplicationSettingForConnectionStringException(SqlSnapshotStoreConnectionNameApplicationKey);
			ConnectionStringSettings connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[applicationKey];
			if (connectionString == null)
				throw new MissingConnectionStringException(applicationKey);
			connectionStringKey = connectionString.ConnectionString;

			string tableName;
			if (!string.IsNullOrWhiteSpace(aggregateRootTypeName) && ConfigurationManager.TryGetSetting(string.Format(SqlSnapshotStoreTableNameApplicationKeyPattern, aggregateRootTypeName), out tableName) && !string.IsNullOrEmpty(tableName))
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

			return SqlEventStoreDataContext.New("Snapshots", connectionStringKey);
		}

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of <see cref="Snapshot"/>.
		/// </summary>
		/// <param name="dbDataContext">The <see cref="DbContext"/> to use.</param>
		protected virtual DbSet<EventData> GetEventStoreSnapshotTable(SqlEventStoreDataContext dbDataContext)
		{
			// Get a typed table to run queries.
			return dbDataContext.Set<EventData>();
		}

		/// <summary>
		/// Persist the provided <paramref name="snapshot"/> into SQL Server using the provided <paramref name="dbDataContext"/>.
		/// </summary>
		protected virtual void Add(SqlEventStoreDataContext dbDataContext, Snapshot snapshot)
		{
			Logger.LogDebug("Adding data to the SQL snapshot database", "SqlSnapshotStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				EventData eventData = BuildEventData(snapshot);
				GetEventStoreSnapshotTable(dbDataContext).Add(eventData);
				dbDataContext.SaveChanges();
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
	}
}