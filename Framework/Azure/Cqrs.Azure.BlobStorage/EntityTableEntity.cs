using System.Runtime.Serialization;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage
{
	public class EntityTableEntity<TEntity>
		: TableEntity<TEntity>
			, IEntityTableEntity<TEntity>
		where TEntity : IEntity
	{
		public EntityTableEntity(TEntity entity)
		{
			PartitionKey = StorageStore<object, object>.GetSafeStorageKey(entity.GetType().FullName);
			RowKey = StorageStore<object, object>.GetSafeStorageKey(entity.Rsn.ToString("N"));
			_entity = entity;
			_entityContent = Serialise(Entity);
		}

		public EntityTableEntity()
		{
		}

		private TEntity _entity;

		[DataMember]
		public TEntity Entity
		{
			get { return _entity; }
			set { _entity = value; }
		}

		private string _entityContent;

		[DataMember]
		public string EntityContent
		{
			get
			{
				return _entityContent;
			}
			set
			{
				_entityContent = value;
				_entity = Deserialise(value);
			}
		}
	}
}