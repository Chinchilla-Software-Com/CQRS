namespace Chat.MicroServices.Authentication.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Cqrs.Configuration;
	using Cqrs.Repositories.Queries;
	using Entities;
	using Factories;

	public class CredentialQueryStrategyBuilder : QueryBuilder<CredentialQueryStrategy, CredentialEntity>
	{
		public CredentialQueryStrategyBuilder(IDomainDataStoreFactory dataStoreFactory, IDependencyResolver dependencyResolver)
			: base(dataStoreFactory.GetCredentialDataStore(), dependencyResolver)
		{
		}

		#region Overrides of QueryBuilder<CredentialQueryStrategy, CredentialEntity>

		protected override IQueryable<CredentialEntity> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<CredentialEntity> leftHandQueryable = null)
		{
			CredentialQueryStrategy queryStrategy = GetNullQueryStrategy();
			SortedSet<QueryParameter> parameters = queryPredicate.Parameters;

			IQueryable<CredentialEntity> resultingQueryable;
			if (queryPredicate.Name == GetFunctionName<Guid>(queryStrategy.WithRsn))
			{
				resultingQueryable = GeneratePredicateWithRsn(parameters, leftHandQueryable);
				return resultingQueryable;
			}
			if (queryPredicate.Name == GetFunctionName<Guid>(queryStrategy.WithUserRsn))
			{
				resultingQueryable = GeneratePredicateWithUserRsn(parameters, leftHandQueryable);
				return resultingQueryable;
			}
			if (queryPredicate.Name == GetFunctionName<string>(queryStrategy.WithHash))
			{
				resultingQueryable = GeneratePredicateWithHash(parameters, leftHandQueryable);
				return resultingQueryable;
			}

			resultingQueryable = GetEmptyQueryPredicate();
			return resultingQueryable;
		}

		#endregion

		protected virtual IQueryable<CredentialEntity> GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<CredentialEntity> leftHandQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			IQueryable<CredentialEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<CredentialEntity> resultingQueryable = query.Where
			(
				credential => credential.Rsn == rsn
			);

			return resultingQueryable;
		}

		protected virtual IQueryable<CredentialEntity> GeneratePredicateWithUserRsn(SortedSet<QueryParameter> parameters, IQueryable<CredentialEntity> leftHandQueryable)
		{
			var userRsn = parameters.GetValue<Guid>(0);

			IQueryable<CredentialEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<CredentialEntity> resultingQueryable = query.Where
			(
				credential => credential.UserRsn == userRsn
			);

			return resultingQueryable;
		}

		protected virtual IQueryable<CredentialEntity> GeneratePredicateWithHash(SortedSet<QueryParameter> parameters, IQueryable<CredentialEntity> leftHandQueryable)
		{
			var hash = parameters.GetValue<string>(0);

			IQueryable<CredentialEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<CredentialEntity> resultingQueryable = query.Where
			(
				credential => credential.Hash == hash
			);

			return resultingQueryable;
		}

		protected override IQueryable<CredentialEntity> GetEmptyQueryPredicate()
		{
			return DependencyResolver.Resolve<IDomainDataStoreFactory>().GetCredentialDataStore();
		}
	}
}