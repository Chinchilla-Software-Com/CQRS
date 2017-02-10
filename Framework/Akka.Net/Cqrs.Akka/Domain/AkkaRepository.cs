#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
	public class AkkaRepository<TAuthenticationToken>
		: Repository<TAuthenticationToken>
		, IAkkaRepository<TAuthenticationToken>
	{
		protected IAkkaEventBus<TAuthenticationToken> EventPublisher { get; private set; }

		public AkkaRepository(IAggregateFactory aggregateFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper, IAkkaEventBus<TAuthenticationToken> eventPublisher)
			: base(aggregateFactory, eventStore, publisher, correlationIdHelper)
		{
			EventPublisher = eventPublisher;
		}

		#region Overrides of Repository<TAuthenticationToken>

		protected override TAggregateRoot CreateAggregate<TAggregateRoot>(Guid id)
		{
			var aggregate = AggregateFactory.CreateAggregate<TAggregateRoot>();

			return aggregate;
		}

		#region Overrides of Repository<TAuthenticationToken>

		protected override void PublishEvent(IEvent<TAuthenticationToken> @event)
		{
			Task.Factory.StartNewSafely(() =>
			{
				EventPublisher.Publish(@event);
				base.PublishEvent(@event);
			});
		}

		#endregion

		#endregion
	}
}