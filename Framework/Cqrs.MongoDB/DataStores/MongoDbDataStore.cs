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
using System.Linq;
using System.Linq.Expressions;
using Cqrs.DataStores;
using Chinchilla.Logging;
using MongoDB.Driver;
using Cqrs.Entities;
using System.Threading.Tasks;

namespace Cqrs.MongoDB.DataStores
{
	/// <summary>
	/// A <see cref="IDataStore{TData}"/> that uses MongoDB for storage.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of <see cref="IEntity"/> the <see cref="IDataStore{TData}"/> will contain.</typeparam>
	public class MongoDbDataStore<TData> : IDataStore<TData>
		where TData : Entity
	{
		/// <summary>
		/// Gets or sets the <see cref="IMongoCollection{TData}"/>
		/// </summary>
		protected IMongoCollection<TData> MongoCollection { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates and Initialises a new instance of the <see cref="MongoDbDataStore{TData}"/> class.
		/// </summary>
		public MongoDbDataStore(ILogger logger, IMongoCollection<TData> mongoCollection)
		{
			Logger = logger;
			MongoCollection = mongoCollection;
			// MongoCollection.Database.RequestStart();
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public virtual IEnumerator<TData> GetEnumerator()
		{
			return MongoCollection.AsQueryable().GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>2</filterpriority>
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
		public virtual Expression Expression
		{
			get { return MongoCollection.AsQueryable().Expression; }
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public virtual Type ElementType
		{
			get { return MongoCollection.AsQueryable().ElementType; }
		}

		/// <summary>
		/// Gets the singleResultQuery provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public virtual IQueryProvider Provider
		{
			get { return MongoCollection.AsQueryable().Provider; }
		}

		#endregion

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Add
#else
			async Task AddAsync
#endif
				(TData data)
		{
			Logger.LogDebug("Adding data to the Mongo database", "MongoDbDataStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
#if NET472
				MongoCollection.InsertOne
#else
				await MongoCollection.InsertOneAsync
#endif
					(data);
				DateTime end = DateTime.Now;
				Logger.LogDebug($"Adding data in the Mongo database took {end - start}.", "MongoDbDataStore\\Add");
			}
			finally
			{
				Logger.LogDebug("Adding data to the Mongo database... Done", "MongoDbDataStore\\Add");
			}
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Add
#else
			async Task AddAsync
#endif
				(IEnumerable<TData> data)
		{
			Logger.LogDebug("Adding data collection to the Mongo database", "MongoDbDataStore\\Add");
			try
			{
#if NET472
				MongoCollection.InsertMany
#else
				await MongoCollection.InsertManyAsync
#endif
					(data);
			}
			finally
			{
				Logger.LogDebug("Adding data collection to the Mongo database... Done", "MongoDbDataStore\\Add");
			}
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft) by setting <see cref="Entity.IsDeleted"/> to true
		/// </summary>
		public virtual
#if NET472
			void Remove
#else
			async Task RemoveAsync
#endif
				(TData data)
		{
			Logger.LogDebug("Removing data from the Mongo database", "MongoDbDataStore\\Remove");
			try
			{
				data.IsDeleted = true;
#if NET472
				Update
#else
				await UpdateAsync
#endif
					(data);
			}
			finally
			{
				Logger.LogDebug("Removing data from the Mongo database... Done", "MongoDbDataStore\\Remove");
			}
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Destroy
#else
			async Task DestroyAsync
#endif
				(TData data)
		{
			Logger.LogDebug("Removing data from the Mongo database", "MongoDbDataStore\\Destroy");
			try
			{
				DateTime start = DateTime.Now;
#if NET472
				MongoCollection.DeleteOne
#else
				await MongoCollection.DeleteOneAsync
#endif
					(x => x.Rsn == data.Rsn);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Updating data in the Mongo database took {0}.", end - start), "MongoDbDataStore\\Update");
			}
			finally
			{
				Logger.LogDebug("Removing data from the Mongo database... Done", "MongoDbDataStore\\Destroy");
			}
		}

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void RemoveAll
#else
			async Task RemoveAllAsync
#endif
				()
		{
			Logger.LogDebug("Removing all from the Mongo database", "MongoDbDataStore\\RemoveAll");
			try
			{
#if NET472
				MongoCollection.DeleteMany
#else
				await MongoCollection.DeleteManyAsync
#endif
					(x => true);
			}
			finally
			{
				Logger.LogDebug("Removing all from the Mongo database... Done", "MongoDbDataStore\\RemoveAll");
			}
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public virtual
#if NET472
			void Update
#else
			async Task UpdateAsync
#endif
				(TData data)
		{
			Logger.LogDebug("Updating data in the Mongo database", "MongoDbDataStore\\Update");
			try
			{
				DateTime start = DateTime.Now;
#if NET472
				MongoCollection.ReplaceOne
#else
				await MongoCollection.ReplaceOneAsync
#endif
					(x => x.Rsn == data.Rsn, data);
				DateTime end = DateTime.Now;
				Logger.LogDebug($"Updating data in the Mongo database took {end - start}.", "MongoDbDataStore\\Update");
			}
			finally
			{
				Logger.LogDebug("Updating data to the Mongo database... Done", "MongoDbDataStore\\Update");
			}
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			// MongoCollection.Database.RequestDone();
		}

		#endregion

		/// <summary>
		/// Executes the "repairDatabase" command on the current database.
		/// </summary>
		public virtual void Repair()
		{
			Logger.LogDebug("Repairing the Mongo database", "MongoDbDataStore\\Repair");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.Database.RunCommand(new JsonCommand<object>("{ repairDatabase: 1 }"));
				DateTime end = DateTime.Now;
				Logger.LogDebug($"Repairing the Mongo database took {end - start}.", "MongoDbDataStore\\Repair");
			}
			finally
			{
				Logger.LogDebug("Repairing the Mongo database... Done", "MongoDbDataStore\\Repair");
			}
		}
	}
}