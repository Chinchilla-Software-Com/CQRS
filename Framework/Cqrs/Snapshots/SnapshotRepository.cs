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
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Snapshots
{
	/// <summary>
	/// Provides basic repository methods for operations with instances of <see cref="IAggregateRoot{TAuthenticationToken}"/>
	/// utilising <see cref="Snapshot">snapshots</see> for optimised rehydration.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public class SnapshotRepository<TAuthenticationToken> : IAggregateRepository<TAuthenticationToken>
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
		public SnapshotRepository(ISnapshotStore snapshotStore, ISnapshotStrategy<TAuthenticationToken> snapshotStrategy, IAggregateRepository<TAuthenticationToken> repository, IEventStore<TAuthenticationToken> eventStore, IAggregateFactory aggregateFactory)
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
			TryMakeSnapshot(aggregate);
			Repository.Save(aggregate, expectedVersion);
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
			if (SnapshotStrategy.IsSnapshotable(typeof(TAggregateRoot)))
			{
				Snapshot snapshot = SnapshotStore.Get<TAggregateRoot>(id);
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
		protected virtual void TryMakeSnapshot(IAggregateRoot<TAuthenticationToken> aggregate)
		{
			if (!SnapshotStrategy.ShouldMakeSnapShot(aggregate))
				return;
			dynamic snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
			var rsnapshot = snapshot as Snapshot;
			if (rsnapshot != null)
			{
				rsnapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
				SnapshotStore.Save(rsnapshot);
			}
			else
			{
				snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
				SnapshotStore.Save(snapshot);
			}
		}
	}
}