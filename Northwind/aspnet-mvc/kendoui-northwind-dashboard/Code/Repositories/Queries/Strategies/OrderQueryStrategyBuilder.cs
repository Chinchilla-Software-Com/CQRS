namespace KendoUI.Northwind.Dashboard.Code.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Cqrs.Configuration;
	using Cqrs.Repositories.Queries;
	using Entities;
	using Factories;

	public class OrderQueryStrategyBuilder : QueryBuilder<OrderQueryStrategy, OrderEntity>
	{
		public OrderQueryStrategyBuilder(IDomainDataStoreFactory dataStoreFactory, IDependencyResolver dependencyResolver)
			: base(dataStoreFactory.GetOrderDataStore(), dependencyResolver)
		{
		}

		#region Overrides of QueryBuilder<OrderQueryStrategy, OrderEntity>

		protected override IQueryable<OrderEntity> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<OrderEntity> leftHandQueryable = null)
		{
			OrderQueryStrategy queryStrategy = GetNullQueryStrategy();
			SortedSet<QueryParameter> parameters = queryPredicate.Parameters;

			IQueryable<OrderEntity> resultingQueryable;
			if (queryPredicate.Name == GetFunctionName<Guid>(queryStrategy.WithRsn))
			{
				resultingQueryable = GeneratePredicateWithRsn(parameters, leftHandQueryable);
				return resultingQueryable;
			}
			if (queryPredicate.Name == GetFunctionName<int>(queryStrategy.WithOrderId))
			{
				resultingQueryable = GeneratePredicateWithOrderId(parameters, leftHandQueryable);
				return resultingQueryable;
			}

			resultingQueryable = GetEmptyQueryPredicate();
			return resultingQueryable;
		}

		#endregion

		protected virtual IQueryable<OrderEntity> GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<OrderEntity> leftHandQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			IQueryable<OrderEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<OrderEntity> resultingQueryable = query.Where
			(
				order => order.Rsn == rsn
			);

			return resultingQueryable;
		}


		protected virtual IQueryable<OrderEntity> GeneratePredicateWithOrderId(SortedSet<QueryParameter> parameters, IQueryable<OrderEntity> leftHandQueryable)
		{
			var orderId = parameters.GetValue<int>(0);

			IQueryable<OrderEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<OrderEntity> resultingQueryable = query.Where
			(
				order => order.OrderId == orderId
			);

			return resultingQueryable;
		}

		protected override IQueryable<OrderEntity> GetEmptyQueryPredicate()
		{
			return DataStore;
		}
	}
}