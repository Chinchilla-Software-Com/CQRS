namespace Cqrs.Repositories.Queries
{
	public class AndQueryPredicate : IAndQueryPredicate
	{
		#region Implementation of IAndQueryPredicate

		public IQueryPredicate LeftQueryPredicate { get; internal set; }

		public IQueryPredicate RightQueryPredicate { get; internal set; }

		#endregion
	}
}