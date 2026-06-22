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
	/// A query that will produce a result
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of the <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of data in the result collection.</typeparam>
	public class ResultQuery<TQueryStrategy, TData>
		: IQueryWithStrategy<TQueryStrategy>
		, IQueryWithResults<TData>
		where TQueryStrategy : IQueryStrategy 
	{
		/// <summary>
		/// The <typeparamref name="TQueryStrategy"/> to be executed and produce a result.
		/// </summary>
		public TQueryStrategy QueryStrategy { get; set; }

		/// <summary>
		/// The resulting of executing the <see cref="QueryStrategy"/>.
		/// </summary>
		public TData Result { get; set; }
	}
}