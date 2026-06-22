#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Cqrs.Entities;

namespace Cqrs.MongoDB.DataStores.Indexes
{
	/// <summary>
	/// An index for MongoDB.
	/// </summary>
	/// <typeparam name="TEntity">The <see cref="Type"/> of <see cref="IEntity"/> this index is for.</typeparam>
	public abstract class MongoDbIndex<TEntity>
	{
		/// <summary>
		/// Indicates if the index enforces unique values. Defaults to false.
		/// </summary>
		public bool IsUnique { get; protected set; }

		/// <summary>
		/// Indicates if the index is in ascending order or descending. Defaults to true meaning ascending order.
		/// </summary>
		public bool IsAcending { get; protected set; }

		/// <summary>
		/// The name of the index. Default to the class name removing any instances of "Index" and "MongoDbIndex" from the name.
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// The selectors that the index is comprised of.
		/// </summary>
		public IEnumerable<Expression<Func<TEntity, object>>> Selectors { get; protected set; }

		/// <summary>
		/// Instantiate a new instance of <see cref="MongoDbIndex{TEntity}"/>.
		/// </summary>
		protected MongoDbIndex()
		{
			IsUnique = false;
			IsAcending = true;
			Name = GetType()
				.Name
				.Replace("MongoDbIndex", string.Empty)
				.Replace("Index", string.Empty);
		}
	}
}