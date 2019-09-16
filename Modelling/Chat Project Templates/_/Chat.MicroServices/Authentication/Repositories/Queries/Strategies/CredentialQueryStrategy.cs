namespace $safeprojectname$.Authentication.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using Cqrs.Repositories.Queries;

	public class CredentialQueryStrategy : QueryStrategy
	{
		internal IList<Func<int, CredentialQueryStrategy>> SortingList { get; set; }

		public CredentialQueryStrategy()
		{
			SortingList = new List<Func<int, CredentialQueryStrategy>>();
		}

		public virtual CredentialQueryStrategy WithRsn(Guid rsn)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithRsn, rsn));

			return this;
		}

		public virtual CredentialQueryStrategy WithUserRsn(Guid userRsn)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithUserRsn, userRsn));

			return this;
		}

		public virtual CredentialQueryStrategy WithHash(string hash)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithHash, hash));

			return this;
		}

		public virtual CredentialQueryStrategy WithNoDeletedCredentials()
		{
			QueryPredicate = And(IsNotLogicallyDeleted());

			return this;
		}	}
}