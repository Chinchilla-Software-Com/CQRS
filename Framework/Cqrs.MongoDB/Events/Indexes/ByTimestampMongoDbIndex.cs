#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq.Expressions;
using Cqrs.Events;
using Cqrs.MongoDB.DataStores.Indexes;

namespace Cqrs.MongoDB.Events.Indexes
{
	public class ByTimestampMongoDbIndex : MongoDbIndex<EventData>
	{
		public ByTimestampMongoDbIndex()
		{
			Selectors = new Expression<Func<EventData, object>>[]
			{
				entity => entity.Timestamp
			};

			IsUnique = false;
		}
	}
}