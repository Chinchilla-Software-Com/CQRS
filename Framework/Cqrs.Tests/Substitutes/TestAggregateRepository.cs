using System;
using System.Collections.Generic;
using System.Reflection;
using Cqrs.Domain;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregateRepository
		: IAggregateRepository<ISingleSignOnToken>
	{
		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<ISingleSignOnToken>
		{
			Saved = aggregate;
			if (expectedVersion == 100)
			{
				throw new Exception();
			}
		}

		public IAggregateRoot<ISingleSignOnToken> Saved { get; private set; }

		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<ISingleSignOnToken>> events = null)
			where TAggregateRoot : IAggregateRoot<ISingleSignOnToken>
		{
			var obj = (TAggregateRoot)Activator.CreateInstance(typeof(TAggregateRoot), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { aggregateId }, null);
			obj.LoadFromHistory(new[] { new TestAggregateDidSomething { Id = aggregateId, Version = 2 } });
			return obj;
		}

		/// <summary>
		/// Retrieves an <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <typeparamref name="TAggregateRoot"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregateId">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to retrieve.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		public TAggregateRoot GetToVersion<TAggregateRoot>(Guid aggregateId, int version, IList<IEvent<ISingleSignOnToken>> events = null)
			where TAggregateRoot : IAggregateRoot<ISingleSignOnToken>
		{
			return Get<TAggregateRoot>(aggregateId, events);
		}

		/// <summary>
		/// Retrieves an <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <typeparamref name="TAggregateRoot"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregateId">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to retrieve.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		public TAggregateRoot GetToDate<TAggregateRoot>(Guid aggregateId, DateTime versionedDate, IList<IEvent<ISingleSignOnToken>> events = null)
			where TAggregateRoot : IAggregateRoot<ISingleSignOnToken>
		{
			return Get<TAggregateRoot>(aggregateId, events);
		}
	}
}