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
	public interface IOrQueryPredicate : IQueryPredicate
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