using System.Collections.Generic;

namespace Cqrs.Repositories.Queries
{
	public class CollectionResultQuery<TQueryStrategy, TData> : ICollectionResultQuery<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy 
	{
		#region Implementation of IQueryWithStrategy<TParameters>

		public TQueryStrategy QueryStrategy { get; set; }

		#endregion

		#region Implementation of IQueryWithResults<out TResult>

		public IEnumerable<TData> Result { get; internal set; }

		#endregion
	}
}