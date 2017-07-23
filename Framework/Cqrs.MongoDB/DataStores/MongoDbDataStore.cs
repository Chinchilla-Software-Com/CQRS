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
using cdmdotnet.Logging;
using MongoDB.Driver;
using Cqrs.Entities;

namespace Cqrs.MongoDB.DataStores
{
	public class MongoDbDataStore<TData> : IDataStore<TData>
		where TData : Entity
	{
		protected IMongoCollection<TData> MongoCollection { get; private set; }

		protected ILogger Logger { get; private set; }

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

		public virtual void Add(TData data)
		{
			Logger.LogDebug("Adding data to the Mongo database", "MongoDbDataStore\\Add");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.InsertOne(data);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Adding data in the Mongo database took {0}.", end - start), "MongoDbDataStore\\Add");
			}
			finally
			{
				Logger.LogDebug("Adding data to the Mongo database... Done", "MongoDbDataStore\\Add");
			}
		}

		public virtual void Add(IEnumerable<TData> data)
		{
			Logger.LogDebug("Adding data collection to the Mongo database", "MongoDbDataStore\\Add");
			try
			{
				MongoCollection.InsertMany(data);
			}
			finally
			{
				Logger.LogDebug("Adding data collection to the Mongo database... Done", "MongoDbDataStore\\Add");
			}
		}

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft) by setting <see cref="Entity.IsLogicallyDeleted"/> to true
		/// </summary>
		public virtual void Remove(TData data)
		{
			Logger.LogDebug("Removing data from the Mongo database", "MongoDbDataStore\\Remove");
			try
			{
				data.IsLogicallyDeleted = true;
				Update(data);
			}
			finally
			{
				Logger.LogDebug("Removing data from the Mongo database... Done", "MongoDbDataStore\\Remove");
			}
		}

		public virtual void Destroy(TData data)
		{
			Logger.LogDebug("Removing data from the Mongo database", "MongoDbDataStore\\Destroy");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.DeleteOne(x => x.Rsn == data.Rsn);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Updating data in the Mongo database took {0}.", end - start), "MongoDbDataStore\\Update");
			}
			finally
			{
				Logger.LogDebug("Removing data from the Mongo database... Done", "MongoDbDataStore\\Destroy");
			}
		}

		public virtual void RemoveAll()
		{
			Logger.LogDebug("Removing all from the Mongo database", "MongoDbDataStore\\RemoveAll");
			try
			{
				MongoCollection.DeleteMany(x => true);
			}
			finally
			{
				Logger.LogDebug("Removing all from the Mongo database... Done", "MongoDbDataStore\\RemoveAll");
			}
		}

		public virtual void Update(TData data)
		{
			Logger.LogDebug("Updating data in the Mongo database", "MongoDbDataStore\\Update");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.ReplaceOne(x => x.Rsn == data.Rsn, data);
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Updating data in the Mongo database took {0}.", end - start), "MongoDbDataStore\\Update");
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
		public void Repair()
		{
			Logger.LogDebug("Repairing the Mongo database", "MongoDbDataStore\\Repair");
			try
			{
				DateTime start = DateTime.Now;
				MongoCollection.Database.RunCommand(new JsonCommand<object>("{ repairDatabase: 1 }"));
				DateTime end = DateTime.Now;
				Logger.LogDebug(string.Format("Repairing the Mongo database took {0}.", end - start), "MongoDbDataStore\\Repair");
			}
			finally
			{
				Logger.LogDebug("Repairing the Mongo database... Done", "MongoDbDataStore\\Repair");
			}
		}
	}
}