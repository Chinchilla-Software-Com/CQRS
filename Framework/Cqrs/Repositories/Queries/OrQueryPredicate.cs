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
	/// An <see cref="IQueryPredicate"/> where either the <see cref="LeftQueryPredicate"/> or the <see cref="RightQueryPredicate"/> must evaluate to true.
	/// </summary>
	public class OrQueryPredicate : IAndQueryPredicate
	{
		#region Implementation of IOrQueryPredicate

		/// <summary>
		/// The left side <see cref="IQueryPredicate"/>.
		/// </summary>
		public IQueryPredicate LeftQueryPredicate { get; internal set; }

		/// <summary>
		/// The right side <see cref="IQueryPredicate"/>.
		/// </summary>
		public IQueryPredicate RightQueryPredicate { get; internal set; }

		#endregion
	}
}