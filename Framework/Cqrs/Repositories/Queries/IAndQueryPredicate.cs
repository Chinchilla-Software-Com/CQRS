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
	/// An <see cref="IQueryPredicate"/> where both the <see cref="LeftQueryPredicate"/> and the <see cref="RightQueryPredicate"/> must evaluate to true.
	/// </summary>
	public interface IAndQueryPredicate : IQueryPredicate
	{
		/// <summary>
		/// The left side <see cref="IQueryPredicate"/>.
		/// </summary>
		IQueryPredicate LeftQueryPredicate { get; }

		/// <summary>
		/// The right side <see cref="IQueryPredicate"/>.
		/// </summary>
		IQueryPredicate RightQueryPredicate { get; }
	}
}