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
using Cqrs.Akka.Domain;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Infrastructure;
using Cqrs.Snapshots;

namespace Cqrs.Akka.Snapshots
{
	/// <summary>
	/// Provides basic repository methods for operations with instances of <see cref="IAggregateRoot{TAuthenticationToken}"/>
	/// utilising <see cref="Snapshot">snapshots</see> for optimised rehydration.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public class AkkaSnapshotRepository<TAuthenticationToken>
		: IAkkaSnapshotAggregateRepository<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or sets the <see cref="ISnapshotStore"/>.
		/// </summary>
		protected ISnapshotStore SnapshotStore { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ISnapshotStrategy{TAuthenticationToken}"/>.
		/// </summary>
		protected ISnapshotStrategy<TAuthenticationToken> SnapshotStrategy { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAggregateRepository{TAuthenticationToken}"/>.
		/// </summary>
		protected IAggregateRepository<TAuthenticationToken> Repository { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IEventStore{TAuthenticationToken}"/>.
		/// </summary>
		protected IEventStore<TAuthenticationToken> EventStore { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAggregateFactory"/>.
		/// </summary>
		protected IAggregateFactory AggregateFactory { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="SnapshotRepository{TAuthenticationToken}"/>.
		/// </summary>
		public AkkaSnapshotRepository(ISnapshotStore snapshotStore, ISnapshotStrategy<TAuthenticationToken> snapshotStrategy, IAggregateRepository<TAuthenticationToken> repository, IEventStore<TAuthenticationToken> eventStore, IAggregateFactory aggregateFactory)
		{
			SnapshotStore = snapshotStore;
			SnapshotStrategy = snapshotStrategy;
			Repository = repository;
			EventStore = eventStore;
			AggregateFactory = aggregateFactory;
		}

		/// <summary>
		/// Calls <see cref="TryMakeSnapshot"/> then <see cref="IAggregateRepository{TAuthenticationToken}.Save{TAggregateRoot}"/> on <see cref="Repository"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregate">The <see cref="IAggregateRoot{TAuthenticationToken}"/> to save and persist.</param>
		/// <param name="expectedVersion">The version number the <see cref="IAggregateRoot{TAuthenticationToken}"/> is expected to be at.</param>
		public void Save<TAggregateRoot>(TAggregateRoot aggregate, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			// We need to grab these first as the changes will have been commitedd already by the time we go to make the snapshot.
			IEnumerable<IEvent<TAuthenticationToken>> uncommittedChanges = aggregate.GetUncommittedChanges();
			// Save the evets first then snapshot the system.
			Repository.Save(aggregate, expectedVersion);
			TryMakeSnapshot(aggregate, uncommittedChanges);
		}

		/// <summary>
		/// Retrieves an <see cref="IAggregateRoot{TAuthenticationToken}"/> of type <typeparamref name="TAggregateRoot"/>,
		/// First using <see cref="TryRestoreAggregateFromSnapshot{TAggregateRoot}"/>, otherwise via <see cref="IAggregateRepository{TAuthenticationToken}.Get{TAggregateRoot}"/> on <see cref="Repository"/>
		/// Then does rehydration.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregateId">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to retrieve.</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		public TAggregateRoot Get<TAggregateRoot>(Guid aggregateId, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			var aggregate = AggregateFactory.Create<TAggregateRoot>();
			int snapshotVersion = TryRestoreAggregateFromSnapshot(aggregateId, aggregate);
			if (snapshotVersion == -1)
			{
				return Repository.Get<TAggregateRoot>(aggregateId);
			}
			IEnumerable<IEvent<TAuthenticationToken>> theseEvents = events ?? EventStore.Get<TAggregateRoot>(aggregateId, false, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
			aggregate.LoadFromHistory(theseEvents);

			return aggregate;
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
		public TAggregateRoot GetToVersion<TAggregateRoot>(Guid aggregateId, int version, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			throw new InvalidOperationException("Verion replay is not appriopriate with snapshots.");
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
		public TAggregateRoot GetToDate<TAggregateRoot>(Guid aggregateId, DateTime versionedDate, IList<IEvent<TAuthenticationToken>> events = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			throw new InvalidOperationException("Verion replay is not appriopriate with snapshots.");
		}

		/// <summary>
		/// Calls <see cref="ISnapshotStrategy{TAuthenticationToken}.IsSnapshotable"/> on <see cref="SnapshotStrategy"/>
		/// If the <typeparamref name="TAggregateRoot"/> is snapshot-able <see cref="ISnapshotStore.Get{TAggregateRoot}"/> is called on <see cref="SnapshotStore"/>.
		/// The Restore method is then called on
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> to restore, since the <paramref name="aggregate"/> may be completely uninitialised.</param>
		/// <param name="aggregate">The <typeparamref name="TAggregateRoot"/></param>
		/// <returns>-1 if no restoration was made, otherwise version number the <typeparamref name="TAggregateRoot"/> was rehydrated to.</returns>
		/// <remarks>There may be more events after the snapshot that still need to rehydrated into the <typeparamref name="TAggregateRoot"/> after restoration.</remarks>
		protected virtual int TryRestoreAggregateFromSnapshot<TAggregateRoot>(Guid id, TAggregateRoot aggregate)
		{
			int version = -1;
			if (SnapshotStrategy.IsSnapshotable(aggregate.GetType()))
			{
				Snapshot snapshot = SnapshotStore.Get(aggregate.GetType(), id);
				if (snapshot != null)
				{
					aggregate.AsDynamic().Restore(snapshot);
					version = snapshot.Version;
				}
			}
			return version;
		}

		/// <summary>
		/// Calls <see cref="ISnapshotStrategy{TAuthenticationToken}.ShouldMakeSnapShot"/> on <see cref="SnapshotStrategy"/>
		/// If the <see cref="IAggregateRoot{TAuthenticationToken}"/> is snapshot-able <see cref="SnapshotAggregateRoot{TAuthenticationToken,TSnapshot}.GetSnapshot"/> is called
		/// The <see cref="Snapshot.Version"/> is calculated, finally <see cref="ISnapshotStore.Save"/> is called on <see cref="SnapshotStore"/>.
		/// </summary>
		/// <param name="aggregate">The <see cref="IAggregateRoot{TAuthenticationToken}"/> to try and snapshot.</param>
		/// <param name="uncommittedChanges">A collection of uncommited changes to assess. If null the aggregate will be asked to provide them.</param>
		protected virtual void TryMakeSnapshot(IAggregateRoot<TAuthenticationToken> aggregate, IEnumerable<IEvent<TAuthenticationToken>> uncommittedChanges)
		{
			if (!SnapshotStrategy.ShouldMakeSnapShot(aggregate, uncommittedChanges))
				return;
			dynamic snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
			var rsnapshot = snapshot as Snapshot;
			if (rsnapshot != null)
			{
				rsnapshot.Version = aggregate.Version;
				SnapshotStore.Save(rsnapshot);
			}
			else
			{
				snapshot.Version = aggregate.Version;
				SnapshotStore.Save(snapshot);
			}
		}

		#region Implementation of IAkkaAggregateRepository<TAuthenticationToken>

		/// <summary>
		/// If <paramref name="events"/> is null, loads the events from <see cref="IEventStore{TAuthenticationToken}"/>, checks for duplicates and then
		/// rehydrates the <paramref name="aggregate"/> with the events.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/>.</typeparam>
		/// <param name="aggregate">The <typeparamref name="TAggregateRoot"/> to rehydrate.</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		/// <param name="throwExceptionOnNoEvents">If true will throw an instance of <see cref="AggregateNotFoundException{TAggregateRoot,TAuthenticationToken}"/> if no aggregate events or provided or found in the <see cref="IEventStore{TAuthenticationToken}"/>.</param>
		public void LoadAggregateHistory<TAggregateRoot>(TAggregateRoot aggregate, IList<IEvent<TAuthenticationToken>> events = null, bool throwExceptionOnNoEvents = true)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			int snapshotVersion = TryRestoreAggregateFromSnapshot(aggregate.Id, aggregate);

			IEnumerable<IEvent<TAuthenticationToken>> theseEvents = events ?? EventStore.Get(aggregate.GetType(), aggregate.Id, false, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
			aggregate.LoadFromHistory(theseEvents);
		}

		#endregion
	}
}