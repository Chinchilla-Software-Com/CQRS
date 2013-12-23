using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public class EventStore : IEventStore
	{
		protected IEventBuilder EventBuilder { get; set; }

		protected IEventDeserialiser EventDeserialiser { get; set; }

		protected IEventStoreConnectionHelper EventStoreConnectionHelper { get; set; }

		public EventStore(IEventBuilder eventBuilder, IEventDeserialiser eventDeserialiser, IEventStoreConnectionHelper eventStoreConnectionHelper)
		{
			EventBuilder = eventBuilder;
			EventDeserialiser = eventDeserialiser;
			EventStoreConnectionHelper = eventStoreConnectionHelper;
		}

		#region Implementation of IEventStore

		public void Save(IEvent @event)
		{
			EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
			using (EventStoreConnection connection = EventStoreConnectionHelper.GetEventStoreConnection())
			{
				connection.AppendToStream(string.Format("cqrs stream: {0}", @event.Id), ExpectedVersion.Any, new[] { eventData });
			}
		}

		/// <remarks>
		/// The value of <paramref name="fromVersion"/> is zero based but the internals indexing of the EventStore is offset by <see cref="StreamPosition.FirstClientEvent"/>.
		/// Adjust the value of <paramref name="fromVersion"/> by <see cref="StreamPosition.FirstClientEvent"/>
		/// </remarks>
		public IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion = -1)
		{
			StreamEventsSlice eventCollection;
			int startPosition = StreamPosition.FirstClientEvent;
			if (fromVersion > -1)
				startPosition = fromVersion + StreamPosition.FirstClientEvent;
			using (EventStoreConnection connection = EventStoreConnectionHelper.GetEventStoreConnection())
			{
				eventCollection = connection.ReadStreamEventsForward(string.Format("cqrs stream: {0}", aggregateId), startPosition, 200, false);
			}
			return eventCollection.Events.Select(EventDeserialiser.Deserialise);
		}

		#endregion

		protected virtual void ListenForNotificationsOnConnection(EventStoreConnection connection)
		{
			connection.SubscribeToAll(true, DisplayNotificationArrival, DisplaySubscriptionDropped);
		}

		private void DisplayNotificationArrival(EventStoreSubscription subscription, ResolvedEvent notification)
		{
			RecordedEvent @event = notification.Event;
			const string chatClientEventTypePrefix = "Cqrs EventStore Client ";
			if (/*string.Format("{0}{1}", chatClientEventTypePrefix, ProcessName) == @event.EventType ||*/ !@event.EventType.StartsWith(chatClientEventTypePrefix))
				return;
			Console.WriteLine("{0} : {1}", @event.EventType.Substring(chatClientEventTypePrefix.Length), @event.EventType);
		}

		private void DisplaySubscriptionDropped(EventStoreSubscription subscription, string someString, Exception exception)
		{
			Console.WriteLine("Opps");
		}
	}
}
