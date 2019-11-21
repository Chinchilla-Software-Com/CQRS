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
using MongoDB.Driver.Linq;
using Cqrs.Entities;
using MongoDB.Driver.Builders;

namespace Cqrs.Mongo.DataStores
{
	/// <summary>
	/// A <see cref="IDataStore{TData}"/> that uses MongoDB for storage.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of <see cref="IEntity"/> the <see cref="IDataStore{TData}"/> will contain.</typeparam>
	public class MongoDataStore<TData> : IDataStore<TData>
		where TData : Entity
	{
		/// <summary>
		/// Gets or sets the <see cref="MongoCollection"/>
		/// </summary>
		protected MongoCollection<TData> MongoCollection { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates and Initialises a new instance of the <see cref="MongoDataStore{TData}"/> class.
		/// </summary>
		public MongoDataStore(ILogger logger, MongoCollection<TData> mongoCollection)
		{
			Logger = logger;
			MongoCollection = mongoCollection;
			MongoCollection.Database.RequestStart();
		}

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<TData> GetEnumerator()
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
		public Expression Expression
		{
			get { return MongoCollection.AsQueryable().Expression; }
		}

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
		/// </returns>
		public Type ElementType
		{
			get { return MongoCollection.AsQueryable().ElementType; }
		}

		/// <summary>
		/// Gets the singleResultQuery provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider
		{
			get { return MongoCollection.AsQueryable().Provider; }
		}

		#endregion

		#region Implementation of IDataStore<TData>

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual void Add(TData data)
		{
			Logger.LogDebug("Adding data to the Mongo database", "MongoDataStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.Insert(data);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the Mongo database took {0}.", end - start), "MongoDataStore\\Add");
			}
			finally
			{
				Logger.LogDebug("Adding data to the Mongo database... Done", "MongoDataStore\\Add");
			}
		}

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		public virtual void Add(IEnumerable<TData> data)
		{
			Logger.LogDebug("Adding data collection to the Mongo database", "MongoDataStore\\Add");
			try
			{
				MongoCollection.InsertBatch(data);
			}
			finally
			{
				Logger.LogDebug("Adding data collection to the Mongo database... Done", "MongoDataStore\\Add");
			}
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft) by setting <see cref="Entity.IsDeleted"/> to true
		/// </summary>
		public virtual void Remove(TData data)
		{
			Logger.LogDebug("Removing data from the Mongo database", "MongoDataStore\\Remove");
			try
			{
				data.IsDeleted = true;
				Update(data);
			}
			finally
			{
				Logger.LogDebug("Removing data from the Mongo database... Done", "MongoDataStore\\Remove");
			}
		}

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		public void Destroy(TData data)
		{
			Logger.LogDebug("Destroying data in the Mongo database", "MongoDataStore\\Destroy");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.Remove(Query.EQ("Rsn", data.Rsn));
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Destroying data in the Mongo database took {0}.", end - start), "MongoDataStore\\Destroy");
			}
			finally
			{
				Logger.LogDebug("Destroying data to the Mongo database... Done", "MongoDataStore\\Destroy");
			}
		}

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		public virtual void RemoveAll()
		{
			Logger.LogDebug("Removing all from the Mongo database", "MongoDataStore\\RemoveAll");
			try
			{
				MongoCollection.RemoveAll();
			}
			finally
			{
				Logger.LogDebug("Removing all from the Mongo database... Done", "MongoDataStore\\RemoveAll");
			}
		}

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		public virtual void Update(TData data)
		{
			Logger.LogDebug("Updating data in the Mongo database", "MongoDataStore\\Update");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.Save(data);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Updating data in the Mongo database took {0}.", end - start), "MongoDataStore\\Update");
			}
			finally
			{
				Logger.LogDebug("Updating data to the Mongo database... Done", "MongoDataStore\\Update");
			}
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			MongoCollection.Database.RequestDone();
		}

		#endregion

		/// <summary>
		/// Executes the "repairDatabase" command on the current database.
		/// </summary>
		public void Repair()
		{
			Logger.LogDebug("Repairing the Mongo database", "MongoDataStore\\Repair");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.Database.RunCommand(new CommandDocument("repairDatabase", 1));
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Repairing the Mongo database took {0}.", end - start), "MongoDataStore\\Repair");
			}
			finally
			{
				Logger.LogDebug("Repairing the Mongo database... Done", "MongoDataStore\\Repair");
			}
		}
	}
}