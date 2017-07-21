namespace $safeprojectname$.Authentication.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Cqrs.Configuration;
	using Cqrs.Repositories.Queries;
	using Entities;
	using Factories;

	public class UserQueryStrategyBuilder : QueryBuilder<UserQueryStrategy, UserEntity>
	{
		public UserQueryStrategyBuilder(IDomainDataStoreFactory dataStoreFactory, IDependencyResolver dependencyResolver)
			: base(dataStoreFactory.GetUserDataStore(), dependencyResolver)
		{
		}

		#region Overrides of QueryBuilder<UserQueryStrategy, UserEntity>

		protected override IQueryable<UserEntity> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<UserEntity> leftHandQueryable = null)
		{
			UserQueryStrategy queryStrategy = GetNullQueryStrategy();
			SortedSet<QueryParameter> parameters = queryPredicate.Parameters;

			IQueryable<UserEntity> resultingQueryable;
			if (queryPredicate.Name == GetFunctionName<Guid>(queryStrategy.WithRsn))
			{
				resultingQueryable = GeneratePredicateWithRsn(parameters, leftHandQueryable);
				return resultingQueryable;
			}

			resultingQueryable = GetEmptyQueryPredicate();
			return resultingQueryable;
		}

		#endregion

		protected virtual IQueryable<UserEntity> GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<UserEntity> leftHandQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			IQueryable<UserEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<UserEntity> resultingQueryable = query.Where
			(
				user => user.Rsn == rsn
			);

			return resultingQueryable;
		}

		protected override IQueryable<UserEntity> GetEmptyQueryPredicate()
		{
			return DependencyResolver.Resolve<IDomainDataStoreFactory>().GetUserDataStore();
		}
	}
}