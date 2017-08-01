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

namespace Cqrs.Mongo.DataStores.Indexes
{
	/// <summary>
	/// A <see cref="MongoIndex{TEntity}"/> for <see cref="IEntity.Rsn"/>
	/// </summary>
	/// <typeparam name="TEntity">The <see cref="Type"/> of <see cref="IEntity"/> this index is for.</typeparam>
	public abstract class ByRsnMongoIndex<TEntity> : MongoIndex<TEntity>
		where TEntity : Entities.MongoEntity
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="ByRsnMongoIndex{TEntity}"/>.
		/// </summary>
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