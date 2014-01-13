using System.Collections.Generic;

namespace Cqrs.Repositories.Queries
{
	public class QueryPredicate : IQueryPredicateData
	{
		#region Implementation of IQueryPredicateData

		public string Name { get; internal set; }

		public SortedSet<QueryParameter> Parameters { get; internal set; }

		#endregion

		public QueryPredicate()
		{
			Parameters = new SortedSet<QueryParameter>();
		}
	}
}