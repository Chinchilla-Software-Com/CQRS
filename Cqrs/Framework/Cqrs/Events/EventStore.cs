#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

namespace Cqrs.Events
{
	public abstract class EventStore<TAuthenticationToken> : IEventStore<TAuthenticationToken>
	{
		protected const string CqrsEventStoreStreamNamePattern = "{0}/{1}";

		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; set; }

		protected IEventDeserialiser<TAuthenticationToken> EventDeserialiser { get; set; }

		public EventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser)
		{
			EventBuilder = eventBuilder;
			EventDeserialiser = eventDeserialiser;
		}

		public virtual void Save<T>(IEvent<TAuthenticationToken> @event)
		{
			Save(@event, typeof(T));
		}

		public virtual void Save(IEvent<TAuthenticationToken> @event, Type aggregateRootType)
		{
			EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, @event.Id);
			eventData.AggregateId = streamName;
			PersitEvent(eventData);
		}

		public abstract IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1);

		protected abstract void PersitEvent(EventData eventData);
	}
}