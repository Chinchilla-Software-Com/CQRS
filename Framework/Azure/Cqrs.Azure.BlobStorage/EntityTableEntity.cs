#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage
{
	/// <summary>
	/// A projection/entity especially designed to work with Azure Table storage.
	/// </summary>
	[Serializable]
	[DataContract]
	public class EntityTableEntity<TEntity>
		: TableEntity<TEntity>
		, IEntityTableEntity<TEntity>
		where TEntity : IEntity
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="EntityTableEntity{TEntity}"/> specificly setting <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.PartitionKey"/> and <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.RowKey"/>.
		/// </summary>
		public EntityTableEntity(TEntity entity)
		{
			PartitionKey = StorageStore<object, object>.GetSafeStorageKey(entity.GetType().FullName);
			RowKey = StorageStore<object, object>.GetSafeStorageKey(entity.Rsn.ToString("N"));
			_entity = entity;
			_entityContent = Serialise(Entity);
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="EntityTableEntity{TEntity}"/>.
		/// </summary>
		public EntityTableEntity()
		{
		}

		private TEntity _entity;

		/// <summary>
		/// Gets or sets the <typeparamref name="TEntity"/>.
		/// </summary>
		[DataMember]
		public TEntity Entity
		{
			get { return _entity; }
			set { _entity = value; }
		}

		private string _entityContent;

		/// <summary>
		/// Gets or sets a serialised version.
		/// </summary>
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