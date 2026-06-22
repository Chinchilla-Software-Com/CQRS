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
	public class QueryPredicate : IQueryPredicateData
	{
		#region Implementation of IQueryPredicateData

		/// <summary>
		/// The Name of the function in the <see cref="IQueryStrategy"/>.
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// The parameters passed to function in the <see cref="IQueryStrategy"/>.
		/// </summary>
		public SortedSet<QueryParameter> Parameters { get; internal set; }

		#endregion

		/// <summary>
		/// Instantiates a new instance of <see cref="QueryPredicate"/>.
		/// </summary>
		public QueryPredicate()
		{
			Parameters = new SortedSet<QueryParameter>();
		}
	}
}