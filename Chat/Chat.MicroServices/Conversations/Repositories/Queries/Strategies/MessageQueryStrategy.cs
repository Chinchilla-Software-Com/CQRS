namespace Chat.MicroServices.Conversations.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using Cqrs.Repositories.Queries;

	public class MessageQueryStrategy : QueryStrategy
	{
		internal IList<Func<int, MessageQueryStrategy>> SortingList { get; set; }

		public MessageQueryStrategy()
		{
			SortingList = new List<Func<int, MessageQueryStrategy>>();
		}

		public virtual MessageQueryStrategy WithRsn(Guid rsn)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithRsn, rsn));

			return this;
		}

		public virtual MessageQueryStrategy WithNoDeletedMessages()
		{
			QueryPredicate = And(IsNotLogicallyDeleted());

			return this;
		}

		public virtual MessageQueryStrategy WithConversationRsn(Guid conversationRsn)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(BuildQueryPredicate(WithConversationRsn, conversationRsn));

			return this;
		}

		public virtual MessageQueryStrategy OrderByDatePosted(int priority = -1)
		{
			if (priority < 0)
				SortingList.Add(OrderByDatePosted);
			else
				SortingList.Insert(priority, OrderByDatePosted);

			return this;
		}
	}
}