using System.Linq;

namespace MyCompany.MyProject.Domain.Authentication.Repositories.Queries.Strategies
{
	public partial class UserQueryStrategyBuilder
	{
		#region Overrides of QueryBuilder<UserQueryStrategy,UserEntity>

		protected override IQueryable<Entities.UserEntity> GetEmptyQueryPredicate()
		{
			return DataStore;
		}

		#endregion
	}
}
