#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.DataStores;
using Cqrs.Repositories;
using Cqrs.Entities;
using Cqrs.Repositories.Queries;
using System.Threading.Tasks;

namespace Cqrs.Azure.Storage.Repositories
{
	/// <summary>
	/// Provides basic repository methods for operations with an <see cref="IDataStore{TData}"/> using Azure Table Storage.
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TQueryBuilder">The <see cref="Type"/> of the <see cref="QueryBuilder{TQueryStrategy, TData}"/> that will be used to build queries.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of data held in storage.</typeparam>
	public class TableStorageRepository<TQueryStrategy, TQueryBuilder, TData> : Repository<TQueryStrategy, TQueryBuilder, TData>
		where TQueryStrategy : IQueryStrategy
		where TQueryBuilder : QueryBuilder<TQueryStrategy, TData>
		where TData : Entity, new()
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="TableStorageRepository{TQueryStrategy,TQueryBuilder,TData}"/>
		/// </summary>
		public TableStorageRepository(Func<IDataStore<TData>> createDataStoreFunction, TQueryBuilder queryBuilder)
			: base(createDataStoreFunction, queryBuilder)
		{
		}

		#region Overrides of Repository<TQueryStrategy,TQueryBuilder,TData>

		/// <summary>
		/// Load the <typeparamref name="TData"/> from Azure Table Storage identified by the provided <paramref name="rsn"/>.
		/// </summary>
		/// <param name="rsn">The identifier if the <typeparamref name="TData"/> to load.</param>
		/// <param name="throwExceptionOnMissingEntity">If true will throw an <see cref="Exception"/> if no data is found in storage.</param>
		public override
#if NET472
			TData Load
#else
			async Task<TData> LoadAsync
#endif
				(Guid rsn, bool throwExceptionOnMissingEntity = true)
		{
			using (IDataStore<TData> dataStore = CreateDataStoreFunction())
			{
				TData result = null;
				try
				{
					result = dataStore.GetByKeyAndRow(rsn);
				}
				catch (InvalidOperationException exception)
				{
					if (throwExceptionOnMissingEntity && (exception.Message == "Sequence contains no elements" || exception.Message.StartsWith("No Table Entity found by")))
						throw exception;
				}

				return
#if NET472
					result;
#else
					await Task.FromResult(result);
#endif
			}
		}

		#endregion
	}
}