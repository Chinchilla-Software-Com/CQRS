namespace Chat.MicroServices.Authentication.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using Cqrs.Repositories.Queries;

	public class UserQueryStrategy : QueryStrategy
	{
		internal IList<Func<int, UserQueryStrategy>> SortingList { get; set; }

		public UserQueryStrategy()
		{
			SortingList = new List<Func<int, UserQueryStrategy>>();
		}

		public virtual UserQueryStrategy WithRsn(Guid rsn)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithRsn, rsn));

			return this;
		}

		public virtual UserQueryStrategy WithNoDeletedUsers()
		{
			QueryPredicate = And(IsNotLogicallyDeleted());

			return this;
		}	}
}