using System;
using System.Collections.Generic;
using Cqrs.Bus;
using Cqrs.Events;
using Cqrs.Messages;
using EventStore.ClientAPI;

namespace Cqrs.EventStore.Bus
{
	public class EventStoreEventPublisher<TAuthenticationToken> : IEventPublisher<TAuthenticationToken>
	{
		protected Dictionary<Type, List<Action<IMessage>>> Routes { get; private set; }

		protected IEventStoreConnection EventStoreConnection { get; private set; }

		protected IStoreLastEventProcessed LastEventProcessedStore { get; private set; }

		public EventStoreEventPublisher(IEventStoreConnectionHelper eventStoreConnectionHelper, IStoreLastEventProcessed lastEventProcessedStore)
		{
			EventStoreConnection = eventStoreConnectionHelper.GetEventStoreConnection();
			LastEventProcessedStore = lastEventProcessedStore;
			Routes = new Dictionary<Type, List<Action<IMessage>>>();
		}

		#region Implementation of IEventPublisher<TAuthenticationToken>

		public void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			List<Action<IMessage>> handlers;
			if (!Routes.TryGetValue(@event.GetType(), out handlers))
				return;
			foreach (Action<IMessage> handler in handlers)
				handler(@event);
		}

		public void Publish<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<TAuthenticationToken>
		{
			foreach (TEvent @event in events)
				Publish(@event);
		}

		#endregion

		protected void InitialiseCatchUpSubscription()
		{
			Position position = GetLastEventProcessedLocation();

			EventStoreConnection.SubscribeToAllFrom(position, false, OnEventAppeared, OnLiveProcessingStarted, OnSubscriptionDropped);
		}

		private Position GetLastEventProcessedLocation()
		{
			return EventStoreUtilities.FormattedStringToPosition(LastEventProcessedStore.EventLocation);
		}

		private void OnEventAppeared(EventStoreCatchUpSubscription catchUpSubscription, ResolvedEvent resolvedEvent)
		{
			if (resolvedEvent.OriginalEvent.EventStreamId != EventStoreBasedLastEventProcessedStore.EventsProcessedStreamName)
			{
				RecordedEvent receivedEvent = resolvedEvent.OriginalEvent;
				// this.logProvider.Log(string.Format("Event appeared: number {0}, position {1}, type {2}", receivedEvent.EventNumber, resolvedEvent.OriginalPosition, receivedEvent.EventType), LogMessageLevel.Info);

				var eventToSend = EventConverter.GetEventFromData<IEvent<TAuthenticationToken>>(resolvedEvent.Event.Data, resolvedEvent.Event.EventType);

				Publish(eventToSend);

				SaveLastEventProcessedLocation(resolvedEvent.OriginalPosition.Value);
			}
		}

		private void SaveLastEventProcessedLocation(Position position)
		{
			LastEventProcessedStore.EventLocation = EventStoreUtilities.PositionToFormattedString(position);
		}

		private void OnLiveProcessingStarted(EventStoreCatchUpSubscription catchUpSubscription)
		{
			// this.logProvider.Log("Subscription live processing started", LogMessageLevel.Info);
		}

		private void OnSubscriptionDropped(EventStoreCatchUpSubscription catchUpSubscription, SubscriptionDropReason dropReason, Exception exception)
		{
			string innerExceptionMessage = string.Empty;
			if (exception != null && exception.InnerException != null)
			{
				innerExceptionMessage = string.Format(" ({0})", exception.InnerException.Message);
			}

			// logProvider.Log("Subscription dropped (reason: " + SubscriptionDropReasonText(dropReason) + innerExceptionMessage + ")", LogMessageLevel.Info);

			if (dropReason == SubscriptionDropReason.ProcessingQueueOverflow)
			{
				// This happens when the server detects that _liveQueue.Count >= MaxPushQueueSize which defaults to 10,000
				// In the forum James Nugent suggests "Wait and reconnect probably with back off" https://gist.github.com/jen20/6092666

				// For now we will just re-subscribe
				InitialiseCatchUpSubscription();
			}

			if (SubscriptionDropMayBeRecoverable(dropReason))
			{
				InitialiseCatchUpSubscription();
			}
		}

		private static bool SubscriptionDropMayBeRecoverable(SubscriptionDropReason dropReason)
		{
			return !(dropReason == SubscriptionDropReason.AccessDenied ||
					 dropReason == SubscriptionDropReason.NotAuthenticated ||
					 dropReason == SubscriptionDropReason.UserInitiated);
		}

		private static string SubscriptionDropReasonText(SubscriptionDropReason reason)
		{
			string reasonText;
			switch (reason)
			{
				case SubscriptionDropReason.UserInitiated:
					reasonText = "User Initiated";
					break;
				case SubscriptionDropReason.NotAuthenticated:
					reasonText = "Not Authenticated";
					break;
				case SubscriptionDropReason.AccessDenied:
					reasonText = "Access Denied";
					break;
				case SubscriptionDropReason.SubscribingError:
					reasonText = "Subscribing Error";
					break;
				case SubscriptionDropReason.ServerError:
					reasonText = "Server Error";
					break;
				case SubscriptionDropReason.ConnectionClosed:
					reasonText = "Connection Closed";
					break;
				case SubscriptionDropReason.CatchUpError:
					reasonText = "CatchUp Error";
					break;
				case SubscriptionDropReason.ProcessingQueueOverflow:
					reasonText = "Processing Queue Overflow";
					break;
				case SubscriptionDropReason.EventHandlerException:
					reasonText = "Event Handler Exception";
					break;
				case SubscriptionDropReason.Unknown:
				default:
					reasonText = "Unknown";
					break;
			}

			return reasonText;
		}
	}
}