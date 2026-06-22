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
	/// A <see cref="MongoDbIndex{TEntity}"/> for <see cref="EventData.Timestamp"/> and <see cref="EventData.EventType"/>.
	/// </summary>
	public class ByTimestampAndEventTypeMongoDbIndex : MongoDbIndex<MongoDbEventData>
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="ByTimestampAndEventTypeMongoDbIndex"/>.
		/// </summary>
		public ByTimestampAndEventTypeMongoDbIndex()
		{
			Selectors = new Expression<Func<MongoDbEventData, object>>[]
			{
				entity => entity.Timestamp,
				entity => entity.EventType
			};

			IsUnique = false;
		}
	}
}