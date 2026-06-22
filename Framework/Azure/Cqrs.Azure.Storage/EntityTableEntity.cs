#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Azure.Data.Tables;
using Cqrs.Entities;

namespace Cqrs.Azure.Storage
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
		/// Instantiates a new instance of <see cref="EntityTableEntity{TEntity}"/> specificly setting <see cref="ITableEntity.PartitionKey"/> and <see cref="ITableEntity.RowKey"/>.
		/// </summary>
		public EntityTableEntity(TEntity entity)
		{
			((ITableEntity)this).PartitionKey = StorageStore<object, object, object>.GetSafeStorageKey(entity.GetType().FullName);
			((ITableEntity)this).RowKey = StorageStore<object, object, object>.GetSafeStorageKey(entity.Rsn.ToString("N"));
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
		[IgnoreDataMember]
		public TEntity Entity
		{
			get { return _entity; }
			set
			{
				_entity = value;
				_entityContent = Serialise(value);
			}
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