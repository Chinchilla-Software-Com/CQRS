#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// Builds an <see cref="IQueryable"/> from a <typeparamref name="TQueryStrategy"/>.
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of the <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of data to query.</typeparam>
	public interface IQueryBuilder<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
	{
		/// <summary>
		/// Create an <see cref="IQueryable"/> of <typeparamref name="TData"/>
		/// that expects a single <typeparamref name="TData"/> item.
		/// </summary>
		/// <param name="singleResultQuery">The query.</param>
		IQueryable<TData> CreateQueryable(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery);

		/// <summary>
		/// Create an <see cref="IQueryable"/> of <typeparamref name="TData"/>
		/// that expects a collection of <typeparamref name="TData"/> items.
		/// </summary>
		/// <param name="collectionResultQuery">The query.</param>
		IQueryable<TData> CreateQueryable(ICollectionResultQuery<TQueryStrategy, TData> collectionResultQuery);
	}
}