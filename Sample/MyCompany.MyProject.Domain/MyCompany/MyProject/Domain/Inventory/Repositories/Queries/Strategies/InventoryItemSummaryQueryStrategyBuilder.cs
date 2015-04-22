using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Repositories.Queries;
using MyCompany.MyProject.Domain.Inventory.Entities;

namespace MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies
{
	public partial class InventoryItemSummaryQueryStrategyBuilder
	{
		partial void GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<InventoryItemSummaryEntity> leftHandQueryable, ref IQueryable<InventoryItemSummaryEntity> resultingQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			var query = (resultingQueryable ?? leftHandQueryable ?? GetEmptyQueryPredicate());

			resultingQueryable = query.Where
			(
				inventoryItem => inventoryItem.Rsn == rsn
			);
		}

		#region Overrides of QueryBuilder<InventoryItemSummaryQueryStrategy,InventoryItemSummaryEntity>

		protected override IQueryable<InventoryItemSummaryEntity> GetEmptyQueryPredicate()
		{
			return DataStore;
		}

		#endregion
	}
}