#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Cqrs.MongoDB.DataStores.Indexes
{
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

		public IEnumerable<Expression<Func<TEntity, object>>> Selectors { get; protected set; }

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