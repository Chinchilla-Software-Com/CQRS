namespace Chat.MicroServices.Conversations.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Cqrs.Configuration;
	using Cqrs.Repositories.Queries;
	using Entities;
	using Factories;

	public class MessageQueryStrategyBuilder : QueryBuilder<MessageQueryStrategy, MessageEntity>
	{
		public MessageQueryStrategyBuilder(IDomainDataStoreFactory dataStoreFactory, IDependencyResolver dependencyResolver)
			: base(dataStoreFactory.GetMessageDataStore(), dependencyResolver)
		{
		}

		#region Overrides of QueryBuilder<MessageQueryStrategy, MessageEntity>

		protected override IQueryable<MessageEntity> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<MessageEntity> leftHandQueryable = null)
		{
			MessageQueryStrategy queryStrategy = GetNullQueryStrategy();
			SortedSet<QueryParameter> parameters = queryPredicate.Parameters;

			IQueryable<MessageEntity> resultingQueryable;
			if (queryPredicate.Name == GetFunctionName<Guid>(queryStrategy.WithRsn))
			{
				resultingQueryable = GeneratePredicateWithRsn(parameters, leftHandQueryable);
				return resultingQueryable;
			}
			if (queryPredicate.Name == GetFunctionName<Guid>(queryStrategy.WithConversationRsn))
			{
				resultingQueryable = GeneratePredicateWithConversationRsn(parameters, leftHandQueryable);
				return resultingQueryable;
			}

			resultingQueryable = GetEmptyQueryPredicate();
			return resultingQueryable;
		}

		#endregion

		#region Overrides of QueryBuilder<MessageQueryStrategy,MessageEntity>

		protected override void ApplySorting(MessageQueryStrategy queryStrategy, ref IQueryable<MessageEntity> queryable)
		{
			var orderQueryable = (IOrderedQueryable<MessageEntity>)queryable;

			int index = 0;
			foreach (Func<int, MessageQueryStrategy> sortingMethod in queryStrategy.SortingList)
			{
				if (sortingMethod.Method.Name == GetFunctionName<int>(queryStrategy.OrderByDatePosted))
				{
					orderQueryable = ApplyOrderByDatePosted(orderQueryable, index++);
				}
			}
			queryable = orderQueryable;
		}

		#endregion

		protected virtual IQueryable<MessageEntity> GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<MessageEntity> leftHandQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			IQueryable<MessageEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<MessageEntity> resultingQueryable = query.Where
			(
				message => message.Rsn == rsn
			);

			return resultingQueryable;
		}

		protected virtual IQueryable<MessageEntity> GeneratePredicateWithConversationRsn(SortedSet<QueryParameter> parameters, IQueryable<MessageEntity> leftHandQueryable)
		{
			var conversationRsn = parameters.GetValue<Guid>(0);

			IQueryable<MessageEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<MessageEntity> resultingQueryable = query.Where
			(
				message => message.ConversationRsn == conversationRsn
			);

			return resultingQueryable;
		}

		protected virtual IOrderedQueryable<MessageEntity> ApplyOrderByDatePosted(IOrderedQueryable<MessageEntity> queryable, int index)
		{
			if (index == 0)
				return queryable.OrderBy(message => message.DatePosted);
			return queryable.ThenBy(message => message.DatePosted);
		}

		protected override IQueryable<MessageEntity> GetEmptyQueryPredicate()
		{
			return DependencyResolver.Resolve<IDomainDataStoreFactory>().GetMessageDataStore();
		}
	}
}