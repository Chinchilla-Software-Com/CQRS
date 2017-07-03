#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Repositories.Queries;

namespace Cqrs.Repositories
{
	public interface IRepository<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
	{
		void Create(TData data);

		void Create(IEnumerable<TData> data);

		ISingleResultQuery<TQueryStrategy, TData> Retrieve(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery, bool throwExceptionWhenNoQueryResults = true);

		ICollectionResultQuery<TQueryStrategy, TData> Retrieve(ICollectionResultQuery<TQueryStrategy, TData> resultQuery);

		void Update(TData data);

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		void Delete(TData data);

		void DeleteAll();

		void Destroy(TData data);

		TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true);
	}
}