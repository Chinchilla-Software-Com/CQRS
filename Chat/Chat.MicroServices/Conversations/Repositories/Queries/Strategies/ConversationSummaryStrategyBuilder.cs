namespace Chat.MicroServices.Conversations.Repositories.Queries.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Cqrs.Configuration;
	using Cqrs.Repositories.Queries;
	using Entities;
	using Factories;

	public class ConversationSummaryQueryStrategyBuilder : QueryBuilder<ConversationSummaryQueryStrategy, ConversationSummaryEntity>
	{
		public ConversationSummaryQueryStrategyBuilder(IDomainDataStoreFactory dataStoreFactory, IDependencyResolver dependencyResolver)
			: base(dataStoreFactory.GetConversationSummaryDataStore(), dependencyResolver)
		{
		}

		#region Overrides of QueryBuilder<ConversationSummaryQueryStrategy, ConversationSummaryEntity>

		protected override IQueryable<ConversationSummaryEntity> GeneratePredicate(QueryPredicate queryPredicate, IQueryable<ConversationSummaryEntity> leftHandQueryable = null)
		{
			ConversationSummaryQueryStrategy queryStrategy = GetNullQueryStrategy();
			SortedSet<QueryParameter> parameters = queryPredicate.Parameters;

			IQueryable<ConversationSummaryEntity> resultingQueryable;
			if (queryPredicate.Name == GetFunctionName<Guid>(queryStrategy.WithRsn))
			{
				resultingQueryable = GeneratePredicateWithRsn(parameters, leftHandQueryable);
				return resultingQueryable;
			}

			resultingQueryable = GetEmptyQueryPredicate();
			return resultingQueryable;
		}

		#endregion

		protected virtual IQueryable<ConversationSummaryEntity> GeneratePredicateWithRsn(SortedSet<QueryParameter> parameters, IQueryable<ConversationSummaryEntity> leftHandQueryable)
		{
			var rsn = parameters.GetValue<Guid>(0);

			IQueryable<ConversationSummaryEntity> query = (leftHandQueryable ?? GetEmptyQueryPredicate());

			IQueryable<ConversationSummaryEntity> resultingQueryable = query.Where
			(
				conversationSummary => conversationSummary.Rsn == rsn
			);

			return resultingQueryable;
		}

		protected override IQueryable<ConversationSummaryEntity> GetEmptyQueryPredicate()
		{
			return DataStore;
		}
	}
}