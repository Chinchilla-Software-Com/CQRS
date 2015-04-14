using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Repositories.Queries;

namespace MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies
{
	public partial class InventoryItemSummaryQueryStrategyBuilder
	{

		partial void GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<Entities.InventoryItemSummaryEntity> leftHandQueryable, ref IQueryable<Entities.InventoryItemSummaryEntity> resultingQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			var query = (resultingQueryable ?? leftHandQueryable ?? DataStore);

			resultingQueryable = query.Where
			(
				meetingInstance => meetingInstance.Rsn == rsn
			);
		}
	}
}