using System.Collections.Generic;

namespace Cqrs.Repositories.Queries
{
	public interface IQueryPredicateData : IQueryPredicate
	{
		string Name { get; }

		SortedSet<QueryParameter> Parameters { get; }
	}
}