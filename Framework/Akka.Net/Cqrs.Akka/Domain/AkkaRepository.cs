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
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Akka.Domain
{
	public class AkkaRepository<TAuthenticationToken> : Repository<TAuthenticationToken>
	{
		public AkkaRepository(IAggregateFactory aggregateFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper)
			: base(aggregateFactory, eventStore, publisher, correlationIdHelper)
		{
		}

		#region Overrides of Repository<TAuthenticationToken>

		protected override TAggregateRoot LoadAggregate<TAggregateRoot>(Guid id, IList<IEvent<TAuthenticationToken>> events = null)
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>();

			/*
			IList<IEvent<TAuthenticationToken>> theseEvents = events ?? EventStore.Get<TAggregateRoot>(id).ToList();
			if (!theseEvents.Any())
				throw new AggregateNotFoundException<TAggregateRoot, TAuthenticationToken>(id);

			var duplicatedEvents =
				theseEvents.GroupBy(x => x.Version)
					.Select(x => new { Version = x.Key, Total = x.Count() })
					.FirstOrDefault(x => x.Total > 1);
			if (duplicatedEvents != null)
				throw new DuplicateEventException<TAggregateRoot, TAuthenticationToken>(id, duplicatedEvents.Version);

			aggregate.LoadFromHistory(theseEvents);
			*/
			return aggregate;
		}

		#endregion
	}
}