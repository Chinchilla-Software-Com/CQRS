using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public class EventStore<TAuthenticationToken> : IEventStore<TAuthenticationToken>
	{
		private const string CqrsEventStoreStreamNamePattern = "{0}/{1}";

		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; set; }

		protected IEventDeserialiser<TAuthenticationToken> EventDeserialiser { get; set; }

		protected IEventStoreConnection EventStoreConnection { get; set; }

		public EventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, IEventStoreConnectionHelper eventStoreConnectionHelper)
		{
			EventBuilder = eventBuilder;
			EventDeserialiser = eventDeserialiser;
			EventStoreConnection = eventStoreConnectionHelper.GetEventStoreConnection();
		}

		#region Implementation of IEventStore

		public void Save<T>(IEvent<TAuthenticationToken> @event)
		{
			Save(@event, typeof (T));
		}

		public void Save(IEvent<TAuthenticationToken> @event, Type aggregateRootType)
		{
			EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
			EventStoreConnection.AppendToStream(string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, @event.Id), ExpectedVersion.Any, new[] { eventData });
		}

		/// <remarks>
		/// The value of <paramref name="fromVersion"/> is zero based but the internals indexing of the EventStore is offset by <see cref="StreamPosition.Start"/>.
		/// Adjust the value of <paramref name="fromVersion"/> by <see cref="StreamPosition.Start"/>
		/// </remarks>
		public IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, int fromVersion = -1)
		{
			int startPosition = StreamPosition.Start;
			if (fromVersion > -1)
				startPosition = fromVersion + StreamPosition.Start;
			StreamEventsSlice eventCollection = EventStoreConnection.ReadStreamEventsForward(string.Format(CqrsEventStoreStreamNamePattern, typeof(T).FullName, aggregateId), startPosition, 200, false);
			return eventCollection.Events.Select(EventDeserialiser.Deserialise);
		}

		#endregion

		protected virtual void ListenForNotificationsOnConnection(IEventStoreConnection connection)
		{
			connection.SubscribeToAll(true, DisplayNotificationArrival, DisplaySubscriptionDropped);
		}

		private void DisplayNotificationArrival(EventStoreSubscription subscription, ResolvedEvent notification)
		{
			RecordedEvent @event = notification.Event;
			string eventTypePrefix = @event.Data.GetType().AssemblyQualifiedName;
			if (string.IsNullOrWhiteSpace(@event.EventType) || @event.EventType != eventTypePrefix)
				return;
			Console.WriteLine("{0} : {1}", eventTypePrefix, @event.EventType);
		}

		private void DisplaySubscriptionDropped(EventStoreSubscription subscription, SubscriptionDropReason reasonDropped, Exception exception)
		{
			Console.WriteLine("Opps");
		}
	}
}
