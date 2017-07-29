#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// A query that will produce a result that contains a collection of <typeparamref name="TData"/> items.
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of the <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of data in the result collection.</typeparam>
	public interface ICollectionResultQuery<TQueryStrategy, out TData>
		: IQueryWithStrategy<TQueryStrategy>, IQueryWithResults<IEnumerable<TData>>
		where TQueryStrategy : IQueryStrategy 
	{
	}
}