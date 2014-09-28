namespace Cqrs.Repositories.Queries
{
	public class OrQueryPredicate : IAndQueryPredicate
	{
		#region Implementation of IOrQueryPredicate

		public IQueryPredicate LeftQueryPredicate { get; internal set; }

		public IQueryPredicate RightQueryPredicate { get; internal set; }

		#endregion
	}
}