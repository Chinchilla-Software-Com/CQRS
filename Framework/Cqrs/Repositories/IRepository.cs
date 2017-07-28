#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
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
		void Create(TData data);

		/// <summary>
		/// Create the newly provided <paramref name="data"/> to storage.
		/// </summary>
		void Create(IEnumerable<TData> data);

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
		void Update(TData data);

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		void Delete(TData data);

		/// <summary>
		/// Delete all contents (normally by use of a truncate operation) in storage.
		/// </summary>
		void DeleteAll();

		/// <summary>
		/// Remove the provided <paramref name="data"/> from storage.
		/// </summary>
		void Destroy(TData data);

		/// <summary>
		/// Load the <typeparamref name="TData"/> from storage identified by the provided <paramref name="rsn"/>.
		/// </summary>
		/// <param name="rsn">The identifier if the <typeparamref name="TData"/> to load.</param>
		/// <param name="throwExceptionOnMissingEntity">If true will throw an <see cref="Exception"/> if no data is found in storage.</param>
		TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true);
	}
}