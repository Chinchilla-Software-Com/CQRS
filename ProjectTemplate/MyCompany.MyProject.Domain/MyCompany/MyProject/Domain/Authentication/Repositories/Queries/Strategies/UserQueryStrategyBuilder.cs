using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Repositories.Queries;

namespace MyCompany.MyProject.Domain.Authentication.Repositories.Queries.Strategies
{
	public partial class UserQueryStrategyBuilder
	{
		partial void GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<Entities.UserEntity> leftHandQueryable, ref IQueryable<Entities.UserEntity> resultingQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			var query = (resultingQueryable ?? leftHandQueryable ?? GetEmptyQueryPredicate());

			resultingQueryable = query.Where
			(
				user => user.Rsn == rsn
			);
		}

		#region Overrides of QueryBuilder<UserQueryStrategy,UserEntity>

		protected override IQueryable<Entities.UserEntity> GetEmptyQueryPredicate()
		{
			return DataStore;
		}

		#endregion
	}
}
