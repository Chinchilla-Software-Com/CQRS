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
	/// A factory to create new instances of <see cref="ResultQuery{TQueryStrategy,TData}"/>.
	/// </summary>
	public interface IQueryFactory
	{
		/// <summary>
		/// Creates a new <see cref="ISingleResultQuery{TQueryStrategy,TData}"/>
		/// </summary>
		/// <typeparam name="TUserQueryStrategy">The <see cref="Type"/> of <see cref="IQueryStrategy"/> it will use.</typeparam>
		/// <typeparam name="TData">The <see cref="Type"/> of data the <see cref="ISingleResultQuery{TQueryStrategy,TData}"/> will operate on.</typeparam>
		ISingleResultQuery<TUserQueryStrategy, TData> CreateNewSingleResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy;

		/// <summary>
		/// Creates a new <see cref="ICollectionResultQuery{TQueryStrategy,TData}"/>
		/// </summary>
		/// <typeparam name="TUserQueryStrategy">The <see cref="Type"/> of <see cref="IQueryStrategy"/> it will use.</typeparam>
		/// <typeparam name="TData">The <see cref="Type"/> of data the <see cref="ISingleResultQuery{TQueryStrategy,TData}"/> will operate on.</typeparam>
		ICollectionResultQuery<TUserQueryStrategy, TData> CreateNewCollectionResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy;
	}
}