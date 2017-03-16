namespace Cqrs.Azure.BlobStorage
{
	public interface IEntityTableEntity<TEntity>
	{
		TEntity Entity { get; set; }
	}
}