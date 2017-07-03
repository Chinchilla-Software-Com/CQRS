#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Akka.Events;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Akka.Domain
{
	public class AkkaSagaRepository<TAuthenticationToken>
		: SagaRepository<TAuthenticationToken>
		, IAkkaSagaRepository<TAuthenticationToken>
	{
		protected IAkkaEventPublisherProxy<TAuthenticationToken> EventPublisher { get; private set; }

		public AkkaSagaRepository(IAggregateFactory aggregateFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper, IAkkaEventPublisherProxy<TAuthenticationToken> eventPublisher)
			: base(aggregateFactory, eventStore, publisher, correlationIdHelper)
		{
			EventPublisher = eventPublisher;
		}

		#region Overrides of Repository<TAuthenticationToken>

		protected override TSaga CreateSaga<TSaga>(Guid id)
		{
			var saga = SagaFactory.Create<TSaga>();

			return saga;
		}

		protected override void PublishEvent(ISagaEvent<TAuthenticationToken> @event)
		{
			Task.Factory.StartNewSafely(() =>
			{
				EventPublisher.Publish(@event);
				base.PublishEvent(@event);
			});
		}

		#endregion
	}
}