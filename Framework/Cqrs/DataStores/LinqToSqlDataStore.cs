#if NET40_OR_GREATER

#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Entities;

namespace Cqrs.DataStores
{
	/// <summary>
	/// A <see cref="IDataStore{TData}"/> using simplified SQL.
	/// </summary>
	public class LinqToSqlDataStore<TData> : IDataStore<TData>
		where TData : Entity
	{
		internal const string SqlDataStoreConnectionNameApplicationKey = @"Cqrs.SqlDataStore.ConnectionStringName";

		internal const string SqlReadableDataStoreConnectionStringKey = "Cqrs.SqlDataStore.Read.ConnectionStringName";

		internal const string SqlWritableDataStoreConnectionStringKey = "Cqrs.SqlDataStore.Write.ConnectionStringName";

		/// <summary />
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Instantiates a new instance of the <see cref="LinqToSqlDataStore{TData}"/> class
		/// </summary>
		public LinqToSqlDataStore(IConfigurationManager configurationManager, ILogger logger)
		{
			ConfigurationManager = configurationManager;
			Logger = logger;
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			DbDataContext = CreateDbDataContext();
			WriteableConnectionStrings = GetWriteableConnectionStrings();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor

			// Get a typed table to run queries.
			Table = DbDataContext.GetTable<TData>();
		}

		/// <summary>
		/// Gets or sets the DataContext.
		/// </summary>
		protected DataContext DbDataContext { get; private set; }

		/// <summary>
		/// Gets or sets the list of writeable connection strings for data mirroring
		/// </summary>
		protected IEnumerable<string> WriteableConnectionStrings { get; private set; }

		/// <summary>
		/// Gets or sets the list of writeable DataContexts for data mirroring
		/// </summary>
		private IList<DataContext> _writeableConnections;

		/// <summary>
		/// Gets or sets the list of writeable DataContexts for data mirroring
		/// </summary>
		protected IEnumerable<DataContext> WriteableConnections
		{
			get
			{
				if (_writeableConnections == null)
				{
					_writeableConnections = new List<DataContext>();
					foreach (string writeableConnectionString in WriteableConnectionStrings)
						_writeableConnections.Add(new DataContext(writeableConnectionString));
				}
				return _writeableConnections;
			}
		}

		/// <summary>
		/// Gets or sets the readable Table
		/// </summary>
		protected Table<TData> Table { get; private set; }

		/// <summary>
		/// Gets or sets the Logger
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Locate the connection settings and create a <see cref="DataContext"/>.
		/// </summary>
		protected virtual DataContext CreateDbDataContext()
		{
			string connectionStringKey;
			string applicationKey;

			// Try read/write connection values first
			if (!ConfigurationManager.TryGetSetting(SqlReadableDataStoreConnectionStringKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
			{
				connectionStringKey = ConfigurationManager.GetConnectionStringBySettingKey(SqlDataStoreConnectionNameApplicationKey, true, true);
				return new DataContext(connectionStringKey);
			}

			try
			{
				connectionStringKey = ConfigurationManager.GetConnectionString(applicationKey);
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No connection string setting named '{0}' was found in the configuration file with the SQL Data Store connection string.", applicationKey), exception);
			}

			return new DataContext(connectionStringKey);
		}

		/// <summary>
		/// Locate the connection settings for persisting data.
		/// </summary>
		protected virtual IEnumerable<string> GetWriteableConnectionStrings()
		{
			Logger.LogDebug("Getting SQL data store writeable connection strings", "LinqToSqlDataStore\\GetWritableConnectionStrings");

			string connectionStringKey;
			string applicationKey;

			// Try read/write connection values first
			if (!ConfigurationManager.TryGetSetting(SqlWritableDataStoreConnectionStringKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
			{
				Logger.LogDebug(string.Format("No application setting named '{0}' was found in the configuration file with the name of a connection string to look for.", SqlWritableDataStoreConnectionStringKey), "LinqToSqlDataStore\\GetWriteableConnectionStrings");
				// Try single connection value
				if (!ConfigurationManager.TryGetSetting(SqlDataStoreConnectionNameApplicationKey, out applicationKey) || string.IsNullOrEmpty(applicationKey))
				{
					throw new NullReferenceException(string.Format("No application setting named '{0}' was found in the configuration file with the name of a connection string to look for.", SqlDataStoreConnectionNameApplicationKey));
				}
			}

			try
			{
				var collection = new List<string>();
				string rootApplicationKey = applicationKey;

				int writeIndex = 1;
				while (!string.IsNullOrWhiteSpace(applicationKey))
				{
					try
					{
						connectionStringKey = ConfigurationManager.GetConnectionString(applicationKey);
						collection.Add(connectionStringKey);
					}
					catch (NullReferenceException exception)
					{
						throw new NullReferenceException(string.Format("No connection string setting named '{0}' was found in the configuration file with a SQL connection string.", applicationKey), exception);
					}

					if (!ConfigurationManager.TryGetSetting(string.Format("{0}.{1}", rootApplicationKey, writeIndex), out applicationKey))
						applicationKey = null;

					writeIndex++;
				}

				if (!collection.Any())
					throw new NullReferenceException();

				return collection;
			}
			catch (NullReferenceException exception)
			{
				throw new NullReferenceException(string.Format("No application setting named '{0}' was found in the configuration file with a SQL connection string.", SqlWritableDataStoreConnectionStringKey), exception);
			}
			finally
			{
				Logger.LogDebug("Getting SQL writeable connection string... Done", "LinqToSqlDataStore\\GetWriteableConnectionStrings");
			}
		}

#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TData> GetEnumerator()
		{
			return Table.AsQueryable().GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

#endregion

#region Implementation of IQueryable

		/// <summary>
		/// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
		/// </returns>
		public Expression Expression
		{
			get { return Table.AsQueryable().Expression; }
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public Type ElementType
		{
			get { return Table.AsQueryable().ElementType; }
		}

		/// <summary>
		/// Gets the query provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider
		{
			get { return Table.AsQueryable().Provider; }
		}

		#endregion

#region Implementation of IDisposable
#pragma warning disable CA1063 // Implement IDisposable Correctly

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			// Dispose of unmanaged resources.
			Dispose(true);
			// Suppress finalization.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose(bool disposing)

		{
			if (disposing)
			{
				Table = null;
				DbDataContext.Dispose();
				DbDataContext = null;
			}
		}

#pragma warning restore CA1063 // Implement IDisposable Correctly
#endregion

#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET40
			void Add
#else
			async Task AddAsync
#endif
			(TData data)
		{
			Logger.LogDebug("Adding data to the SQL database", "LinqToSqlDataStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				using (var transaction = new TransactionScope())
				{
					foreach (DataContext writeableConnection in WriteableConnections)
					{
						Table<TData> table = Table;
						// This optimises for single connection handling
						if (writeableConnection != DbDataContext)
							table = writeableConnection.GetTable<TData>();
						table.InsertOnSubmit(data);
						writeableConnection.SubmitChanges();
					}
					transaction.Complete();
				}
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the SQL database took {0}.", end - start), "LinqToSqlDataStore\\Add");
			}
			finally
			{
				Logger.LogDebug("Adding data to the SQL database... Done", "LinqToSqlDataStore\\Add");
			}
#if NET40
#else
			await Task.CompletedTask;
#endif
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET40
			void Add
#else
			async Task AddAsync
#endif
			(IEnumerable<TData> data)
		{
			Logger.LogDebug("Adding data collection to the SQL database", "LinqToSqlDataStore\\Add\\Collection");
			try
			{
				DateTime start = DateTime.Now;
				// Multiple enumeration optimisation
				data = data.ToList();
				using (var transaction = new TransactionScope())
				{
					foreach (DataContext writeableConnection in WriteableConnections)
					{
						Table<TData> table = Table;
						// This optimises for single connection handling
						if (writeableConnection != DbDataContext)
							table = writeableConnection.GetTable<TData>();
						table.InsertAllOnSubmit(data);
						writeableConnection.SubmitChanges();
					}
					transaction.Complete();
				}
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the SQL database took {0}.", end - start), "LinqToSqlDataStore\\Add\\Collection");
			}
			finally
			{
				Logger.LogDebug("Adding data collection to the SQL database... Done", "LinqToSqlDataStore\\Add\\Collection");
			}
#if NET40
#else
			await Task.CompletedTask;
#endif
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft) deleted by setting <see cref="Entity.IsDeleted"/> to true in the data store and persist the change.
		/// </summary>
		public virtual
#if NET40
			void Remove
#else
			async Task RemoveAsync
#endif
			(TData data)
		{
			Logger.LogDebug("Removing data from the Sql database", "LinqToSqlDataStore\\Remove");
			try
			{
				DateTime start = DateTime.Now;
				data.IsDeleted = true;
#if NET40
				Update
#else
				await UpdateAsync
#endif
					(data);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Removing data from the Sql database took {0}.", end - start), "LinqToSqlDataStore\\Remove");
			}
			finally
			{
				Logger.LogDebug("Removing data from the Sql database... Done", "LinqToSqlDataStore\\Remove");
			}
#if NET40
#else
			await Task.CompletedTask;
#endif
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public virtual
#if NET40
			void Destroy
#else
			async Task DestroyAsync
#endif
			(TData data)
		{
			Logger.LogDebug("Removing data from the SQL database", "LinqToSqlDataStore\\Destroy");
			try
			{
				DateTime start = DateTime.Now;
				using (var transaction = new TransactionScope())
				{
					foreach (DataContext writeableConnection in WriteableConnections)
					{
						Table<TData> table = Table;
						// This optimises for single connection handling
						if (writeableConnection != DbDataContext)
							table = writeableConnection.GetTable<TData>();
						try
						{
							table.DeleteOnSubmit(data);
						}
						catch (InvalidOperationException exception)
						{
							if (exception.Message != "Cannot remove an entity that has not been attached.")
								throw;
							try
							{
								table.Attach(data);
								writeableConnection.Refresh(RefreshMode.KeepCurrentValues, data);
							}
							catch (DuplicateKeyException)
							{
								// We're using the same context apparently
							}
							table.DeleteOnSubmit(data);
						}
						try
						{
							writeableConnection.SubmitChanges();
						}
						catch (ChangeConflictException)
						{
							writeableConnection.Refresh(RefreshMode.KeepCurrentValues, data);
							writeableConnection.SubmitChanges();
						}
					}
					transaction.Complete();
				}
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Removing data from the SQL database took {0}.", end - start), "LinqToSqlDataStore\\Destroy");
			}
			finally
			{
				Logger.LogDebug("Removing data from the SQL database... Done", "LinqToSqlDataStore\\Destroy");
			}
#if NET40
#else
			await Task.CompletedTask;
#endif
		}

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public virtual
#if NET40
			void RemoveAll
#else
			async Task RemoveAllAsync
#endif
			()
		{
			Logger.LogDebug("Removing all from the SQL database", "LinqToSqlDataStore\\RemoveAll");
			try
			{
				using (var transaction = new TransactionScope())
				{
					foreach (DataContext writeableConnection in WriteableConnections)
					{
						Table<TData> table = Table;
						// This optimises for single connection handling
						if (writeableConnection != DbDataContext)
							table = writeableConnection.GetTable<TData>();
						table.Truncate();
						writeableConnection.SubmitChanges();
					}
					transaction.Complete();
				}
			}
			finally
			{
				Logger.LogDebug("Removing all from the SQL database... Done", "LinqToSqlDataStore\\RemoveAll");
			}
#if NET40
#else
			await Task.CompletedTask;
#endif
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public virtual
#if NET40
			void Update
#else
			async Task UpdateAsync
#endif
			(TData data)
		{
			Logger.LogDebug("Updating data in the SQL database", "LinqToSqlDataStore\\Update");
			try
			{
				DateTime start = DateTime.Now;
				using (var transaction = new TransactionScope())
				{
					foreach (DataContext writeableConnection in WriteableConnections)
					{
						Table<TData> table = Table;
						// This optimises for single connection handling
						if (writeableConnection != DbDataContext)
							table = writeableConnection.GetTable<TData>();
						try
						{
							table.Attach(data);
							writeableConnection.Refresh(RefreshMode.KeepCurrentValues, data);
						}
						catch (DuplicateKeyException)
						{
							// We're using the same context apparently
						}
						writeableConnection.SubmitChanges();
					}
					transaction.Complete();
				}
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Updating data in the SQL database took {0}.", end - start), "LinqToSqlDataStore\\Update");
			}
			finally
			{
				Logger.LogDebug("Updating data to the SQL database... Done", "LinqToSqlDataStore\\Update");
			}
#if NET40
#else
			await Task.CompletedTask;
#endif
		}

		#endregion
	}
}
#endif