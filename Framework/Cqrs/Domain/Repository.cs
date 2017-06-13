#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Domain
{
	[Obsolete("Use AggregateRepository")]
	public class Repository<TAuthenticationToken>
		: AggregateRepository<TAuthenticationToken>
		, IRepository<TAuthenticationToken>
	{
		protected IEventStore<TAuthenticationToken> EventStore { get; private set; }

		protected IEventPublisher<TAuthenticationToken> Publisher { get; private set; }

		protected IAggregateFactory AggregateFactory { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		public Repository(IAggregateFactory aggregateFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper)
			: base(aggregateFactory, eventStore, publisher, correlationIdHelper)
		{
		}
	}
}