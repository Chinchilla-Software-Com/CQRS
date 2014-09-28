namespace Cqrs.Repositories.Queries
{
	public class SingleResultQuery<TQueryStrategy, TData> : ISingleResultQuery<TQueryStrategy, TData>
		where TQueryStrategy : IQueryStrategy 
	{
		#region Implementation of IQueryWithStrategy<TParameters>

		public TQueryStrategy QueryStrategy { get; set; }

		#endregion

		#region Implementation of IQueryWithResults<out TResult>

		public TData Result { get; internal set; }

		#endregion
	}
}