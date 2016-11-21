#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq.Expressions;

namespace Cqrs.MongoDB.DataStores.Indexes
{
	public abstract class ByRsnMongoDbIndex<TEntity> : MongoDbIndex<TEntity>
		where TEntity : Entities.MongoEntity
	{
		protected ByRsnMongoDbIndex()
		{
			Selectors = new Expression<Func<TEntity, object>>[]
			{
				entity => entity.Rsn
			};

			IsUnique = true;
		}
	}
}