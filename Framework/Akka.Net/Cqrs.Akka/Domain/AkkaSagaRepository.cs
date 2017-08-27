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
	/// <summary>
	/// A <see cref="SagaRepository{TAuthenticationToken}"/> that is safe to use within Akka.NET
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public class AkkaSagaRepository<TAuthenticationToken>
		: SagaRepository<TAuthenticationToken>
		, IAkkaSagaRepository<TAuthenticationToken>
	{
		/// <summary>
		/// Gets the <see cref="IAkkaEventPublisherProxy{TAuthenticationToken}"/>
		/// </summary>
		protected IAkkaEventPublisherProxy<TAuthenticationToken> EventPublisher { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaSagaRepository{TAuthenticationToken}"/>
		/// </summary>
		public AkkaSagaRepository(IAggregateFactory aggregateFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper, IAkkaEventPublisherProxy<TAuthenticationToken> eventPublisher)
			: base(aggregateFactory, eventStore, publisher, correlationIdHelper)
		{
			EventPublisher = eventPublisher;
		}

		#region Overrides of Repository<TAuthenticationToken>

		/// <summary>
		/// Calls <see cref="IAggregateFactory.Create"/> to get a, <typeparamref name="TSaga"/>.
		/// </summary>
		/// <typeparam name="TSaga">The <see cref="Type"/> of <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
		/// <param name="id">The id of the <typeparamref name="TSaga"/> to create.</param>
		protected override TSaga CreateSaga<TSaga>(Guid id)
		{
			var saga = SagaFactory.Create<TSaga>();

			return saga;
		}

		/// <summary>
		/// Publish the saved <paramref name="event"/> asynchronously on <see cref="EventPublisher"/>,
		/// then calls <see cref="SagaRepository{TAuthenticationToken}.PublishEvent"/>
		/// </summary>
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