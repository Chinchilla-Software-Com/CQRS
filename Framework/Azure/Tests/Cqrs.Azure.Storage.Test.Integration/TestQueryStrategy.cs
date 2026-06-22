#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Repositories.Queries;

namespace Cqrs.Azure.Storage.Test.Integration
{
	/// <summary>
	/// A test <see cref="IQueryStrategy"/>.
	/// </summary>
	public class TestQueryStrategy : IQueryStrategy
	{
		#region Implementation of IQueryStrategy

		/// <summary>
		/// The predicate that will be evaluated.
		/// </summary>
		public IQueryPredicate QueryPredicate { get; set; }

		#endregion
	}
}