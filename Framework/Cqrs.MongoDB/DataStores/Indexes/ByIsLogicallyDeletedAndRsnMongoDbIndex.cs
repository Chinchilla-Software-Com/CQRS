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

namespace Cqrs.MongoDB.DataStores.Indexes
{
	/// <summary>
	/// A <see cref="MongoDbIndex{TEntity}"/> for <see cref="IEntity.IsDeleted"/> and <see cref="IEntity.Rsn"/>
	/// </summary>
	/// <typeparam name="TEntity">The <see cref="Type"/> of <see cref="IEntity"/> this index is for.</typeparam>
	public abstract class ByIsDeletedAndRsnMongoDbIndex<TEntity> : MongoDbIndex<TEntity>
		where TEntity : Entities.MongoEntity
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="ByIsDeletedAndRsnMongoDbIndex{TEntity}"/>.
		/// </summary>
		protected ByIsDeletedAndRsnMongoDbIndex()
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