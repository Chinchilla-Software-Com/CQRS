
using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Repositories.Queries;

namespace Northwind.Domain.Orders.Repositories.Queries.Strategies
{
	public partial class InvoicesQueryStrategyBuilder
	{

		partial void GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<Entities.InvoicesEntity> leftHandQueryable, ref IQueryable<Entities.InvoicesEntity> resultingQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			var query = (resultingQueryable ?? leftHandQueryable ?? GetEmptyQueryPredicate());

			resultingQueryable = query.Where
			(
				invoice => invoice.Rsn == rsn
			);
		}
	}
}
