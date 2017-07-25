#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Bus;
using EventStore.ClientAPI;

namespace Cqrs.EventStore.Bus
{
	/// <summary>
	/// Indicates the position in store where the stream has been read up to.
	/// </summary>
	public class EventStoreBasedLastEventProcessedStore : IStoreLastEventProcessed
	{
		/// <summary>
		/// The name of the event stream use to store the position/location information.
		/// </summary>
		public const string EventsProcessedStreamName = @"EventsProcessed";

		/// <summary>
		/// The name of the event type we use in the event stream to store the position/location information.
		/// </summary>
		public const string EventType = @"ProcessedEvent";

		/// <summary>
		/// The <see cref="IEventStoreConnection"/> used to read and write streams in the Greg Young Event Store.
		/// </summary>
		protected IEventStoreConnection EventStoreConnection { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="EventStoreBasedLastEventProcessedStore"/>.
		/// </summary>
		/// <param name="eventStoreConnection">The <see cref="IEventStoreConnection"/> used to read streams.</param>
		public EventStoreBasedLastEventProcessedStore(IEventStoreConnection eventStoreConnection)
		{
			if (eventStoreConnection == null)
			{
				throw new ArgumentNullException("eventStoreConnection");
			}

			EventStoreConnection = eventStoreConnection;
		}

		/// <summary>
		/// The location within the store where the stream has been read up to.
		/// </summary>
		public string EventLocation
		{
			get
			{
				StreamEventsSlice slice = EventStoreConnection.ReadStreamEventsBackwardAsync(EventsProcessedStreamName, StreamPosition.End, 1, false).Result;
				if (slice.Events.Length > 0)
				{
					return EventStoreUtilities.ByteArrayToString(slice.Events[0].Event.Data);
				}

				return string.Empty;
			}
			set
			{
				var eventData = new EventData(Guid.NewGuid(), EventType, false, EventStoreUtilities.StringToByteArray(value), null);
				EventStoreConnection.AppendToStreamAsync(EventsProcessedStreamName, ExpectedVersion.Any, eventData).RunSynchronously();
			}
		}
	}
}
