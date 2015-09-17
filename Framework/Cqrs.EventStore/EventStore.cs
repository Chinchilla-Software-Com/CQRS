using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Events;
using EventStore.ClientAPI;
using EventData = EventStore.ClientAPI.EventData;

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
			Save(typeof (T), @event);
		}

		public void Save(Type aggregateRootType, IEvent<TAuthenticationToken> @event)
		{
			EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, @event.Id);
			using (EventStoreTransaction transaction = EventStoreConnection.StartTransactionAsync(streamName, ExpectedVersion.Any).Result)
			{
				WriteResult saveResult = EventStoreConnection.AppendToStreamAsync(streamName, ExpectedVersion.Any, new[] {eventData}).Result;
				WriteResult commitResult = transaction.CommitAsync().Result;
			}
		}

		/// <remarks>
		/// The value of <paramref name="fromVersion"/> is zero based but the internals indexing of the EventStore is offset by <see cref="StreamPosition.Start"/>.
		/// Adjust the value of <paramref name="fromVersion"/> by <see cref="StreamPosition.Start"/>
		/// </remarks>
		public IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof (T), aggregateId, useLastEventOnly, fromVersion);
		}

		public IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			int startPosition = StreamPosition.Start;
			if (fromVersion > -1)
				startPosition = fromVersion + StreamPosition.Start;
			StreamEventsSlice eventCollection;
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateType.FullName, aggregateId);
			if (useLastEventOnly)
				eventCollection = EventStoreConnection.ReadStreamEventsBackwardAsync(streamName, startPosition, 1, false).Result;
			else
				eventCollection = EventStoreConnection.ReadStreamEventsForwardAsync(streamName, startPosition, 200, false).Result;
			return eventCollection.Events.Select(EventDeserialiser.Deserialise);
		}

		#endregion

		protected virtual void ListenForNotificationsOnConnection(IEventStoreConnection connection)
		{
			connection.SubscribeToAllAsync(true, DisplayNotificationArrival, DisplaySubscriptionDropped).RunSynchronously();
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
