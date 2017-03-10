using System.Runtime.Serialization;
using Cqrs.Events;

namespace Cqrs.Azure.BlobStorage
{
	public class EventDataTableEntity<TEventData>
		: TableEntity<TEventData>
			, IEventDataTableEntity<TEventData>
		where TEventData : EventData
	{
		public EventDataTableEntity(TEventData eventData, bool isCorrelationIdTableStorageStore = false)
		{
			PartitionKey = StorageStore<object, object>.GetSafeStorageKey(isCorrelationIdTableStorageStore ? eventData.CorrelationId.ToString("N") : eventData.AggregateId);
			RowKey = StorageStore<object, object>.GetSafeStorageKey(eventData.EventId.ToString("N"));
			_eventData = eventData;
			_eventDataContent = Serialise(EventData);
		}

		public EventDataTableEntity()
		{
		}

		private TEventData _eventData;

		[DataMember]
		public TEventData EventData
		{
			get { return _eventData; }
			set { _eventData = value; }
		}

		private string _eventDataContent;

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