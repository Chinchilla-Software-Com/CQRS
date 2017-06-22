namespace KendoUI.Northwind.Dashboard.Code.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using Cqrs.Repositories.Queries;

	public class OrderQueryStrategy : QueryStrategy
	{
		internal IList<Func<int, OrderQueryStrategy>> SortingList { get; set; }

		public OrderQueryStrategy()
		{
			SortingList = new List<Func<int, OrderQueryStrategy>>();
		}

		public virtual OrderQueryStrategy WithRsn(Guid rsn)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithRsn, rsn));

			return this;
		}

		public virtual OrderQueryStrategy WithOrderId(int orderId)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithOrderId, orderId));

			return this;
		}

		public virtual OrderQueryStrategy WithNoDeletedOrders()
		{
			QueryPredicate = And(IsNotLogicallyDeleted());

			return this;
		}
	}
}