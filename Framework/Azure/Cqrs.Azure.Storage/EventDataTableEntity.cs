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
using Cqrs.Events;

namespace Cqrs.Azure.Storage
{
	/// <summary>
	/// A projection/entity especially designed to work with Azure Table storage.
	/// </summary>
	[Serializable]
	[DataContract]
	public class EventDataTableEntity<TEventData>
		: TableEntity<TEventData>
		, IEventDataTableEntity<TEventData>
		where TEventData : EventData
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="EventDataTableEntity{TEntity}"/> specificly setting <see cref="ITableEntity.PartitionKey"/> and <see cref="ITableEntity.RowKey"/>.
		/// </summary>
		public EventDataTableEntity(TEventData eventData, bool isCorrelationIdTableStorageStore = false)
		{
			((ITableEntity)this).PartitionKey = StorageStore<object, object, object>.GetSafeStorageKey(isCorrelationIdTableStorageStore ? eventData.CorrelationId.ToString("N") : eventData.AggregateId);
			((ITableEntity)this).RowKey = StorageStore<object, object, object>.GetSafeStorageKey(eventData.EventId.ToString("N"));
			_eventData = eventData;
			_eventDataContent = Serialise(EventData);
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="EventDataTableEntity{TEntity}"/>.
		/// </summary>
		public EventDataTableEntity()
		{
		}

		private TEventData _eventData;

		/// <summary>
		/// Gets or sets the <typeparamref name="TEventData"/>.
		/// </summary>
		[IgnoreDataMember]
		public TEventData EventData
		{
			get { return _eventData; }
			set { _eventData = value; }
		}

		private string _eventDataContent;

		/// <summary>
		/// Gets or sets a serialised version.
		/// </summary>
		[DataMember]
		public string EventDataContent
		{
			get
			{
				return _eventDataContent;
			}
			set
			{
				_eventDataContent = value;
				_eventData = Deserialise(value);
			}
		}
	}
}