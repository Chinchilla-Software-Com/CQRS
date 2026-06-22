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
	/// A query that utilises a <typeparamref name="TQueryStrategy"/>.
	/// </summary>
	public interface IQueryWithStrategy<TQueryStrategy>
		where TQueryStrategy : IQueryStrategy 
	{
		/// <summary>
		/// The <typeparamref name="TQueryStrategy"/> to be executed and produce a result.
		/// </summary>
		TQueryStrategy QueryStrategy { get; set; }
	}
}