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
			Save(@event, typeof(T));
		}

		public virtual void Save(IEvent<TAuthenticationToken> @event, Type aggregateRootType)
		{
			Logger.LogInfo("Saving aggregate root event", string.Format("{0}\\Save", GetType().Name));
			try
			{
				EventData eventData = EventBuilder.CreateFrameworkEvent(@event);
				string streamName = string.Format(CqrsEventStoreStreamNamePattern, aggregateRootType.FullName, @event.Id);
				eventData.AggregateId = streamName;
				PersitEvent(eventData);
			}
			finally
			{
				Logger.LogInfo("Saving aggregate root event... done", string.Format("{0}\\Save", GetType().Name));
			}
		}

		public abstract IEnumerable<IEvent<TAuthenticationToken>> Get<T>(Guid aggregateId, bool useLastEventOnly = false, int fromVersion = -1);

		protected abstract void PersitEvent(EventData eventData);
	}
}