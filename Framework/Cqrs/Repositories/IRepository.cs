#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cqrs.DataStores;
using Cqrs.Repositories.Queries;

namespace Cqrs.Repositories
{
	/// <summary>
	/// Provides basic repository methods for operations with an <see cref="IDataStore{TData}"/>.
	/// </summary>
	public interface IRepository<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
	{
		/// <summary>
		/// Create the newly provided <paramref name="data"/> to storage.
		/// </summary>
#if NET40
		void Create
#else
		Task CreateAsync
#endif
			(TData data);

		/// <summary>
		/// Create the newly provided <paramref name="data"/> to storage.
		/// </summary>
#if NET40
		void Create
#else
		Task CreateAsync
#endif
			(IEnumerable<TData> data);

		/// <summary>
		/// Builds and executes the provided <paramref name="singleResultQuery"/>.
		/// </summary>
		/// <param name="singleResultQuery">The <see cref="ISingleResultQuery{TQueryStrategy,TData}"/> to build and execute.</param>
		/// <param name="throwExceptionWhenNoQueryResults">If true will throw an <see cref="Exception"/> if no data is found in storage.</param>
		ISingleResultQuery<TQueryStrategy, TData> Retrieve(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery, bool throwExceptionWhenNoQueryResults = true);

		/// <summary>
		/// Builds and executes the provided <paramref name="resultQuery"/>.
		/// </summary>
		/// <param name="resultQuery">The <see cref="ICollectionResultQuery{TQueryStrategy,TData}"/> to build and execute.</param>
		ICollectionResultQuery<TQueryStrategy, TData> Retrieve(ICollectionResultQuery<TQueryStrategy, TData> resultQuery);

		/// <summary>
		/// Update the provided <paramref name="data"/> in storage.
		/// </summary>
#if NET40
		void Update
#else
		Task UpdateAsync
#endif
			(TData data);

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
#if NET40
		void Delete
#else
		Task DeleteAsync
#endif
			(TData data);

		/// <summary>
		/// Delete all contents (normally by use of a truncate operation) in storage.
		/// </summary>
#if NET40
		void DeleteAll
#else
		Task DeleteAllAsync
#endif
			();

		/// <summary>
		/// Remove the provided <paramref name="data"/> from storage.
		/// </summary>
#if NET40
		void Destroy
#else
		Task DestroyAsync
#endif
			(TData data);

		/// <summary>
		/// Load the <typeparamref name="TData"/> from storage identified by the provided <paramref name="rsn"/>.
		/// </summary>
		/// <param name="rsn">The identifier if the <typeparamref name="TData"/> to load.</param>
		/// <param name="throwExceptionOnMissingEntity">If true will throw an <see cref="Exception"/> if no data is found in storage.</param>

#if NET40
		TData Load
#else
		Task<TData> LoadAsync
#endif
			(Guid rsn, bool throwExceptionOnMissingEntity = true);
	}
}