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
using cdmdotnet.Logging;

namespace Cqrs.Events
{
	public abstract class EventStore<TAuthenticationToken> : IEventStore<TAuthenticationToken>
	{
		protected const string CqrsEventStoreStreamNamePattern = "{0}/{1}";

		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; set; }

		protected IEventDeserialiser<TAuthenticationToken> EventDeserialiser { get; set; }

		protected ITelemetryHelper TelemetryHelper { get; set; }

		protected ILogger Logger { get; private set; }

		protected EventStore(IEventBuilder<TAuthenticationToken> eventBuilder, IEventDeserialiser<TAuthenticationToken> eventDeserialiser, ILogger logger)
		{
			EventBuilder = eventBuilder;
			EventDeserialiser = eventDeserialiser;
			Logger = logger;
			TelemetryHelper = new NullTelemetryHelper();
		}

		public virtual void Save<T>(IEvent<TAuthenticationToken> @event)
		{
			Save(typeof(T), @event);
		}

		protected virtual string GenerateStreamName(Type aggregateRootType, IEvent<TAuthenticationToken> @event)
		{
			return GenerateStreamName(aggregateRootType, @event.Id);
		}

		protected virtual string GenerateStreamName(Type aggregateRootType, Guid aggregateId)
		{
			return string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, aggregateId);
		}

		public virtual void Save(Type aggregateRootType, IEvent<TAuthenticationToken> @event)
		{
			Logger.LogDebug(string.Format("Saving aggregate root event type '{0}'", @event.GetType().FullName), string.Format("{0}\\Save", GetType().Name));
			EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
			string streamName = GenerateStreamName(aggregateRootType, @event);
			eventData.AggregateId = streamName;
			eventData.AggregateRsn = @event.Id;
			eventData.Version = @event.Version;
			eventData.CorrelationId = @event.CorrelationId;
			PersistEvent(eventData);
			Logger.LogInfo(string.Format("Saving aggregate root event type '{0}'... done", @event.GetType().FullName), string.Format("{0}\\Save", GetType().Name));
			TelemetryHelper.TrackMetric(string.Format("Cqrs/EventStore/Save/{0}", streamName), 1);
		}

		public virtual IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1)
		{
			IEnumerable<IEvent<TAuthenticationToken>> results = Get(typeof (T), aggregateId, useLastEventOnly, fromVersion);
			TelemetryHelper.TrackMetric(string.Format("Cqrs/EventStore/Get/{0}", GenerateStreamName(typeof(T), aggregateId)), results.Count());

			return results;
		}

		public abstract IEnumerable<IEvent<TAuthenticationToken>> Get(Type aggregateRootType, Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1);

		public abstract IEnumerable<EventData> Get(Guid correlationId);

		protected abstract void PersistEvent(EventData eventData);
	}
}