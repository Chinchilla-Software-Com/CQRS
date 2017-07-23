#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq.Expressions;

namespace Cqrs.Mongo.DataStores.Indexes
{
	public abstract class ByRsnMongoIndex<TEntity> : MongoIndex<TEntity>
		where TEntity : Entities.MongoEntity
	{
		protected ByRsnMongoIndex()
		{
			Selectors = new Expression<Func<TEntity, object>>[]
			{
				entity => entity.Rsn
			};

			IsUnique = true;
		}
	}
}