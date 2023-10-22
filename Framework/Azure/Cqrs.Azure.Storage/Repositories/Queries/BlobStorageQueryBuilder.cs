#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Entities;
using Cqrs.Repositories.Queries;

namespace Cqrs.Azure.Storage.Repositories.Queries
{
	/// <summary>
	/// Builds an <see cref="IQueryable"/> from a <typeparamref name="TQueryStrategy"/> for use with Azure Storage.
	/// </summary>
	/// <typeparam name="TQueryStrategy">The <see cref="Type"/> of the <see cref="IQueryStrategy"/>.</typeparam>
	/// <typeparam name="TData">The <see cref="Type"/> of data to query.</typeparam>
	public abstract class BlobStorageQueryBuilder<TQueryStrategy, TData>
		: QueryBuilder<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy
		where TData : Entity
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="BlobStorageQueryBuilder{TQueryStrategy,TData}"/>.
		/// </summary>
		protected BlobStorageQueryBuilder(IDataStore<TData> dataStore, IDependencyResolver dependencyResolver)
			: base(dataStore, dependencyResolver)
		{
		}

		#region Overrides of QueryBuilder<TQueryStrategy,TData>

		/// <summary>
		/// Returns the folder from <see cref="QueryBuilder{TQueryStrategy,TData}.DataStore"/> itself.
		/// </summary>
		protected override IQueryable<TData> GetEmptyQueryPredicate()
		{
			return DataStore.GetByFolder().AsQueryable();
		}

		#endregion
	}
}