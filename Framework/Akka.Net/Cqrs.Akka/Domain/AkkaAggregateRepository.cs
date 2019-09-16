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
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;

namespace Cqrs.Akka.Domain
{
	/// <summary>
	/// A <see cref="AggregateRepository{TAuthenticationToken}"/> that is safe to use within Akka.NET
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public class AkkaAggregateRepository<TAuthenticationToken>
		: AggregateRepository<TAuthenticationToken>
		, IAkkaAggregateRepository<TAuthenticationToken>
	{
		/// <summary>
		/// Gets the <see cref="IAkkaEventPublisherProxy{TAuthenticationToken}"/>
		/// </summary>
		protected IAkkaEventPublisherProxy<TAuthenticationToken> EventPublisher { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaAggregateRepository{TAuthenticationToken}"/>
		/// </summary>
		public AkkaAggregateRepository(IAggregateFactory aggregateFactory, IEventStore<TAuthenticationToken> eventStore, IEventPublisher<TAuthenticationToken> publisher, ICorrelationIdHelper correlationIdHelper, IConfigurationManager configurationManager, IAkkaEventPublisherProxy<TAuthenticationToken> eventPublisher)
			: base(aggregateFactory, eventStore, publisher, correlationIdHelper, configurationManager)
		{
			EventPublisher = eventPublisher;
		}

		#region Overrides of Repository<TAuthenticationToken>

		/// <summary>
		/// Calls <see cref="IAggregateFactory.Create"/> to get a, <typeparamref name="TAggregateRoot"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="id">The id of the <typeparamref name="TAggregateRoot"/> to create.</param>
		protected override TAggregateRoot CreateAggregate<TAggregateRoot>(Guid id)
		{
			var aggregate = AggregateFactory.Create<TAggregateRoot>();

			return aggregate;
		}

		/// <summary>
		/// Publish the saved <paramref name="event"/> asynchronously on <see cref="EventPublisher"/>,
		/// then calls <see cref="AggregateRepository{TAuthenticationToken}.PublishEvent"/>
		/// </summary>
		protected override void PublishEvent(IEvent<TAuthenticationToken> @event)
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