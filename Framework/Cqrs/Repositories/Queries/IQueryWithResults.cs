namespace Cqrs.Repositories.Queries
{
	public interface IQueryWithResults<out TResult>
	{
		TResult Result { get; }
	}
}