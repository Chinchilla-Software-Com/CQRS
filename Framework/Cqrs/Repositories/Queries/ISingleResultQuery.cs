#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// A query that will produce a result that is a single <typeparamref name="TData"/> item.
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of the <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of result data.</typeparam>
	public interface ISingleResultQuery<TQueryStrategy, out TData>
		: IQueryWithStrategy<TQueryStrategy>, IQueryWithResults<TData>
		where TQueryStrategy : IQueryStrategy 
	{
	}
}