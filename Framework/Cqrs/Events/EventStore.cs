#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using cdmdotnet.Logging;

namespace Cqrs.Events
{
	public abstract class EventStore<TAuthenticationToken> : IEventStore<TAuthenticationToken>
	{
		protected const string CqrsEventStoreStreamNamePattern = "{0}/{1}";

		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; set; }

		protected IEventDeserialiser<TAuthenticationToken> EventDeserialiser { get; set; }

		protected ILogger Logger { get; private set; }

		protected EventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger)
		{
			EventBuilder = eventBuilder;
			EventDeserialiser = eventDeserialiser;
			Logger = logger;
		}

		public virtual void Save<T>(IEvent<TAuthenticationToken> @event)
		{
			Save(typeof(T), @event);
		}

		public virtual void Save(Type aggregateRootType, IEvent<TAuthenticationToken> @event)
		{
			Logger.LogDebug(string.Format("Saving aggregate root event type '{0}'", @event.GetType().FullName), string.Format("{0}\\Save", GetType().Name));
			EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
			string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, @event.Id);
			eventData.AggregateId = streamName;
			eventData.Version = @event.Version;
			eventData.CorrelationId = @event.CorrelationId;
			PersistEvent(eventData);
			Logger.LogInfo(string.Format("Saving aggregate root event type '{0}'... done", @event.GetType().FullName), string.Format("{0}\\Save", GetType().Name));
		}

		public virtual IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			return Get(typeof (T), aggregateId, useLastEventOnly, fromVersion);
		}

		public abstract IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1);

		public abstract IEnumerable<EventData> Get(Guid correlationId);

		protected abstract void PersistEvent(EventData eventData);
	}
}