using System;
using System.Linq.Expressions;

namespace Cqrs.Mongo.DataStores.Indexes
{
	public abstract class ByIsLogicallyDeletedAndRsnMongoIndex<TEntity> : MongoIndex<TEntity>
		where TEntity : Entities.MongoEntity
	{
		protected ByIsLogicallyDeletedAndRsnMongoIndex()
		{
			Selectors = new Expression<Func<TEntity, object>>[]
			{
				entity => entity.IsLogicallyDeleted,
				entity => entity.Rsn
			};
		}
	}
}