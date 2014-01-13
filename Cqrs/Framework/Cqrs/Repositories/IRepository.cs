using System;
using System.Collections.Generic;
using Cqrs.Repositories.Queries;

namespace Cqrs.Repositories
{
	internal interface IRepository<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy, new()
	{
		void Create(TData data);

		void Create(IEnumerable<TData> data);

		ISingleResultQuery<TQueryStrategy, TData> Retrieve(ISingleResultQuery<TQueryStrategy, TData> singleResultQuery, bool throwExceptionWhenNoQueryResults = true);

		ICollectionResultQuery<TQueryStrategy, TData> Retrieve(ICollectionResultQuery<TQueryStrategy, TData> resultQuery);

		void Update(TData data);

		void Delete(TData data);

		void DeleteAll();

		TData Load(Guid rsn, bool throwExceptionOnMissingEntity = true);
	}
}