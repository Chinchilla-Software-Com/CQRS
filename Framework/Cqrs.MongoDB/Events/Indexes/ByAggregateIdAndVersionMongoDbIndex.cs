#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq.Expressions;
using Cqrs.Events;
using Cqrs.MongoDB.DataStores.Indexes;

namespace Cqrs.MongoDB.Events.Indexes
{
	/// <summary>
	/// A <see cref="MongoDbIndex{TEntity}"/> for <see cref="EventData.AggregateId"/> and <see cref="EventData.Version"/>.
	/// </summary>
	public class ByAggregateIdAndVersionMongoDbIndex : MongoDbIndex<MongoDbEventData>
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="ByAggregateIdAndVersionMongoDbIndex"/>.
		/// </summary>
		public ByAggregateIdAndVersionMongoDbIndex()
		{
			Selectors = new Expression<Func<MongoDbEventData, object>>[]
			{
				entity => entity.AggregateId,
				entity => entity.Version
			};

			IsUnique = true;
		}
	}
}