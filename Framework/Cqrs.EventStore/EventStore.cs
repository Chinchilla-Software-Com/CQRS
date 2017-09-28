#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Messages;
using EventStore.ClientAPI;
using EventData = EventStore.ClientAPI.EventData;

namespace Cqrs.EventStore
{
	/// <summary>
	/// A Greg Young Event Store based <see cref="EventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class EventStore<TAuthenticationToken> : IEventStore<TAuthenticationToken>
	{
		/// <summary>
		/// The pattern used to create stream names.
		/// </summary>
		protected const string CqrsEventStoreStreamNamePattern = "{0}/{1}";

		/// <summary>
		/// The <see cref="IEventBuilder{TAuthenticationToken}"/> used to build events.
		/// </summary>
		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; set; }

		/// <summary>
		/// The <see cref="IEventDeserialiser{TAuthenticationToken}"/> used to deserialise events.
		/// </summary>
		protected IEventDeserialiser<TAuthenticationToken> EventDeserialiser { get; set; }

		/// <summary>
		/// The <see cref="IEventStoreConnection"/> used to read and write streams in the Greg Young Event Store.
		/// </summary>
		protected IEventStoreConnection EventStoreConnection { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="EventStore{TAuthenticationToken}"/>.
		/// </summary>
		public EventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, IEventStoreConnectionHelper eventStoreConnectionHelper)
		{
			EventBuilder = eventBuilder;
			EventDeserialiser = eventDeserialiser;
			EventStoreConnection = eventStoreConnectionHelper.GetEventStoreConnection();
		}

		#region Implementation of IEventStore

		/// <summary>
		/// Saves the provided <paramref name="event"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to be saved.</param>
		public void Save<T>(IEvent<TAuthenticationToken> @event)
		{
			Save(typeof (T), @event);
		}

		/// <summary>
		/// Saves the provided <paramref name="event"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to be saved.</param>
		public void Save(Type aggregateRootType, IEvent<TAuthenticationToken> @event)
		{
			EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, @event.GetIdentity());
			using (EventStoreTransaction transaction = EventStoreConnection.StartTransactionAsync(streamName, ExpectedVersion.Any).Result)
			{
				WriteResult saveResult = EventStoreConnection.AppendToStreamAsync(streamName, ExpectedVersion.Any, new[] {eventData}).Result;
				WriteResult commitResult = transaction.CommitAsync().Result;
			}
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <typeparamref name="T">aggregate root</typeparamref> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		/// <remarks>
		/// The value of <paramref name="fromVersion"/> is zero based but the internals indexing of the EventStore is offset by <see cref="StreamPosition.Start"/>.
		/// Adjust the value of <paramref name="fromVersion"/> by <see cref="StreamPosition.Start"/>
		/// </remarks>
		public IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof (T), aggregateId, useLastEventOnly, fromVersion);
		}

		/// <summary>
		/// Gets a collection of <see cref="IEvent{TAuthenticationToken}"/> for the <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <paramref name="aggregateRootType"/> with the ID matching the provided <paramref name="aggregateId"/>.
		/// </summary>
		/// <param name="aggregateRootType"> <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</param>
		/// <param name="aggregateId">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="useLastEventOnly">Loads only the last event<see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="fromVersion">Load events starting from this version</param>
		/// <remarks>
		/// The value of <paramref name="fromVersion"/> is zero based but the internals indexing of the EventStore is offset by <see cref="StreamPosition.Start"/>.
		/// Adjust the value of <paramref name="fromVersion"/> by <see cref="StreamPosition.Start"/>
		/// </remarks>
		public IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			int startPosition = StreamPosition.Start;
			if (fromVersion > -1)
				startPosition = fromVersion + StreamPosition.Start;
			StreamEventsSlice eventCollection;
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);
			if (useLastEventOnly)
				eventCollection = EventStoreConnection.ReadStreamEventsBackwardAsync(streamName, startPosition, 1, false).Result;
			else
				eventCollection = EventStoreConnection.ReadStreamEventsForwardAsync(streamName, startPosition, 200, false).Result;
			return eventCollection.Events.Select(EventDeserialiser.Deserialise);
		}

		/// <summary>
		/// Get all <see cref="IEvent{TAuthenticationToken}"/> instances for the given <paramref name="correlationId"/>.
		/// </summary>
		/// <param name="correlationId">The <see cref="IMessage.CorrelationId"/> of the <see cref="IEvent{TAuthenticationToken}"/> instances to retrieve.</param>
		public IEnumerable<Events.EventData> Get(Guid correlationId)
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		/// Requests the <paramref name="connection"/> responds to OnConnect client notifications.
		/// </summary>
		/// <param name="connection">The <see cref="IEventStoreConnection"/> that will be monitored.</param>
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