using System;
using System.Collections.Generic;

namespace Cqrs.Mongo.DataStores
{
	public abstract class MongoIndex<TEntity>
	{
		/// <summary>
		/// Indicates if the index enforces unique values. Defaults to true.
		/// </summary>
		public bool IsUnique { get; protected set; }

		/// <summary>
		/// Indicates if the index is in ascending order or descending. Defaults to true meaning ascending order.
		/// </summary>
		public bool IsAcending { get; protected set; }

		/// <summary>
		/// The name of the index. Default to the class name removing any instances of "Index" and "MongoIndex" from the name.
		/// </summary>
		public string Name { get; protected set; }

		public IEnumerable<System.Linq.Expressions.Expression<Func<TEntity, object>>> Selectors { get; protected set; }

		protected MongoIndex()
		{
			IsUnique = true;
			IsAcending = true;
			Name = GetType()
				.Name
				.Replace("Index", string.Empty)
				.Replace("MongoIndex", string.Empty);
		}
	}
}