#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq.Expressions;
using Cqrs.Entities;
using Cqrs.Mongo.Entities;

namespace Cqrs.Mongo.DataStores.Indexes
{
	/// <summary>
	/// A <see cref="MongoIndex{TEntity}"/> for <see cref="IEntity.IsLogicallyDeleted"/> and <see cref="IEntity.Rsn"/>
	/// </summary>
	/// <typeparam name="TEntity">The <see cref="Type"/> of <see cref="IEntity"/> this index is for.</typeparam>
	public abstract class ByIsLogicallyDeletedAndRsnMongoIndex<TEntity> : MongoIndex<TEntity>
		where TEntity : MongoEntity
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="ByIsLogicallyDeletedAndRsnMongoIndex{TEntity}"/>.
		/// </summary>
		protected ByIsLogicallyDeletedAndRsnMongoIndex()
		{
			Selectors = new Expression<Func<TEntity, object>>[]
			{
				entity => entity.IsLogicallyDeleted,
				entity => entity.Rsn
			};

			IsUnique = true;
		}
	}
}