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
	/// A <see cref="MongoIndex{TEntity}"/> for <see cref="IEntity.IsDeleted"/> and <see cref="IEntity.Rsn"/>
	/// </summary>
	/// <typeparam name="TEntity">The <see cref="Type"/> of <see cref="IEntity"/> this index is for.</typeparam>
	public abstract class ByIsDeletedAndRsnMongoIndex<TEntity> : MongoIndex<TEntity>
		where TEntity : MongoEntity
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="ByIsDeletedAndRsnMongoIndex{TEntity}"/>.
		/// </summary>
		protected ByIsDeletedAndRsnMongoIndex()
		{
			Selectors = new Expression<Func<TEntity, object>>[]
			{
				entity => entity.IsDeleted,
				entity => entity.Rsn
			};

			IsUnique = true;
		}
	}
}