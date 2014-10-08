using System;
using Cqrs.Bus;
using EventStore.ClientAPI;

namespace Cqrs.EventStore.Bus
{
	public class EventStoreBasedLastEventProcessedStore : IStoreLastEventProcessed
	{
		public const string EventsProcessedStreamName = @"EventsProcessed";

		public const string EventType = @"ProcessedEvent";

		protected IEventStoreConnection EventStoreConnection { get; private set; }

		public EventStoreBasedLastEventProcessedStore(IEventStoreConnection eventStoreConnection)
		{
			if (eventStoreConnection == null)
			{
				throw new ArgumentNullException("eventStoreConnection");
			}

			EventStoreConnection = eventStoreConnection;
		}

		public string EventLocation
		{
			get
			{
				StreamEventsSlice slice = EventStoreConnection.ReadStreamEventsBackward(EventsProcessedStreamName, StreamPosition.End, 1, false);
				if (slice.Events.Length > 0)
				{
					return EventStoreUtilities.ByteArrayToString(slice.Events[0].Event.Data);
				}

				return string.Empty;
			}
			set
			{
				var eventData = new EventData(Guid.NewGuid(), EventType, false, EventStoreUtilities.StringToByteArray(value), null);
				EventStoreConnection.AppendToStream(EventsProcessedStreamName, ExpectedVersion.Any, new [] { eventData });
			}
		}
	}
}
