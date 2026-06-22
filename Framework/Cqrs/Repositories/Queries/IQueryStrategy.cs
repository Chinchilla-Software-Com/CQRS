#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// A specification for a query to execute.
	/// </summary>
	public interface IQueryStrategy
	{
		/// <summary>
		/// The predicate that will be evaluated.
		/// </summary>
		IQueryPredicate QueryPredicate { get; }
	}
}