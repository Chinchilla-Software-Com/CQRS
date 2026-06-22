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

namespace Cqrs.Azure.BlobStorage.Test.Integration
{
	/// <summary>
	/// A test <see cref="QueryBuilder{TQueryStrategy,TData}"/>
	/// </summary>
	public class TestQueryBuilder<TData> : QueryBuilder<TestQueryStrategy, TData>
		where TData : Entity
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="TestQueryBuilder{TData}"/>.
		/// </summary>
		public TestQueryBuilder(IDataStore<TData> dataStore, IDependencyResolver dependencyResolver)
			: base(dataStore, dependencyResolver)
		{
		}

		#region Overrides of QueryBuilder<TestQueryStrategy,TData>

		/// <summary>
		/// Raises a <see cref="NotImplementedException"/> as it's currently not needed.
		/// </summary>
		protected override IQueryable<TData> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<TData> leftHandQueryable = null)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}