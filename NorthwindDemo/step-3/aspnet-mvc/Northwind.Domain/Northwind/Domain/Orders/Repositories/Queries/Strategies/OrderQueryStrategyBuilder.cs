
using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Repositories.Queries;

namespace Northwind.Domain.Orders.Repositories.Queries.Strategies
{
	public partial class OrderQueryStrategyBuilder
	{

		partial void GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<Entities.OrderEntity> leftHandQueryable, ref IQueryable<Entities.OrderEntity> resultingQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			var query = (resultingQueryable ?? leftHandQueryable ?? GetEmptyQueryPredicate());

			resultingQueryable = query.Where
			(
				order => order.Rsn == rsn
			);
		}

		partial void GeneratePredicateWithOrderId(SortedSet<QueryParameter> parameters, IQueryable<Entities.OrderEntity> leftHandQueryable, ref IQueryable<Entities.OrderEntity> resultingQueryable)
		{
			var orderId = parameters.GetValue<int>(0);

			var query = (resultingQueryable ?? leftHandQueryable ?? GetEmptyQueryPredicate());

			resultingQueryable = query.Where
			(
				order => order.OrderId == orderId
			);
		}

		#region Overrides of QueryBuilder<OrderEntityQueryStrategy,Entities.OrderEntity>

		protected override IQueryable<Entities.OrderEntity> GetEmptyQueryPredicate()
		{
			return DataStore;
		}

		#endregion
	}
}
