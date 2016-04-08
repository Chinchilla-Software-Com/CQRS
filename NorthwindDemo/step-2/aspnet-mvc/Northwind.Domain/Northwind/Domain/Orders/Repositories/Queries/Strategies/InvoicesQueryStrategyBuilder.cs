
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
			throw new NotImplementedException();
		}
	}
}
