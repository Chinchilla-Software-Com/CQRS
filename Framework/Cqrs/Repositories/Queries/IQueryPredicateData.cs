#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// Information about a query.
	/// </summary>
	public interface IQueryPredicateData : IQueryPredicate
	{
		/// <summary>
		/// The Name of the function in the <see cref="IQueryStrategy"/>.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The parameters passed to the function in the <see cref="IQueryStrategy"/>.
		/// </summary>
		SortedSet<QueryParameter> Parameters { get; }
	}
}