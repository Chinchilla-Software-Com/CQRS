namespace $safeprojectname$.Conversations.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using Cqrs.Repositories.Queries;

	public class ConversationSummaryQueryStrategy : QueryStrategy
	{
		internal IList<Func<int, ConversationSummaryQueryStrategy>> SortingList { get; set; }

		public ConversationSummaryQueryStrategy()
		{
			SortingList = new List<Func<int, ConversationSummaryQueryStrategy>>();
		}

		public virtual ConversationSummaryQueryStrategy WithRsn(Guid rsn)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithRsn, rsn));

			return this;
		}

		public virtual ConversationSummaryQueryStrategy WithNoDeletedConversations()
		{
			QueryPredicate = And(IsNotLogicallyDeleted());

			return this;
		}
	}
}