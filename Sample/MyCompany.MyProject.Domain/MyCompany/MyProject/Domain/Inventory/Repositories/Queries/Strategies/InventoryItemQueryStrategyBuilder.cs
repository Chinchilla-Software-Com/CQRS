using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Repositories.Queries;
using MyCompany.MyProject.Domain.Inventory.Entities;

namespace MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies
{
	public partial class InventoryItemQueryStrategyBuilder
	{
		partial void GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<InventoryItemEntity> leftHandQueryable, ref IQueryable<InventoryItemEntity> resultingQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			var query = (resultingQueryable ?? leftHandQueryable ?? GetEmptyQueryPredicate());

			resultingQueryable = query.Where
			(
				inventoryItem => inventoryItem.Rsn == rsn
			);
		}

		#region Overrides of QueryBuilder<InventoryItemQueryStrategy,InventoryItemEntity>

		protected override IQueryable<InventoryItemEntity> GetEmptyQueryPredicate()
		{
			return DataStore;
		}

		#endregion
	}
}
