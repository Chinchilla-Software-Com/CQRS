#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Domain.Exceptions;
using Cqrs.Events;

namespace Cqrs.Domain
{
	/// <summary>
	/// Provides a basic container to control when <see cref="IEvent{TAuthenticationToken}">events</see> are store in an <see cref="IEventStore{TAuthenticationToken}"/> and then published on an <see cref="IEventPublisher{TAuthenticationToken}"/>.
	/// </summary>
	/// <remarks>
	/// This shouldn't normally be used as a singleton.
	/// </remarks>
	public class UnitOfWork<TAuthenticationToken>
		: IUnitOfWork<TAuthenticationToken>
	{
		private IAggregateRepository<TAuthenticationToken> Repository { get; set; }

		private Dictionary<Guid, IAggregateDescriptor<TAuthenticationToken>> TrackedAggregates { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="UnitOfWork{TAuthenticationToken}"/>
		/// </summary>
		public UnitOfWork(IAggregateRepository<TAuthenticationToken> repository)
		{
			if(repository == null)
				throw new ArgumentNullException("repository");

			Repository = repository;
			TrackedAggregates = new Dictionary<Guid, IAggregateDescriptor<TAuthenticationToken>>();
		}

		/// <summary>
		/// Add an item into the <see cref="IUnitOfWork{TAuthenticationToken}"/> ready to be committed.
		/// </summary>
		public void Add<TAggregateRoot>(TAggregateRoot aggregate)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			if (!IsTracked(aggregate.Id))
			{
				var aggregateDescriptor = new AggregateDescriptor<TAggregateRoot, TAuthenticationToken>
				{
					Aggregate = aggregate,
					Version = aggregate.Version
				};
				TrackedAggregates.Add(aggregate.Id, aggregateDescriptor);
			}
			else if (((TrackedAggregates[aggregate.Id]).Aggregate) != (IAggregateRoot<TAuthenticationToken>)aggregate)
				throw new ConcurrencyException(aggregate.Id);
		}

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> if it has already been loaded or get it from the <see cref="IAggregateRepository{TAuthenticationToken}"/>.
		/// </summary>
		public TAggregateRoot Get<TAggregateRoot>(Guid id, int? expectedVersion = null, bool useSnapshots = false)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			if(IsTracked(id))
			{
				var trackedAggregate = (TAggregateRoot)TrackedAggregates[id].Aggregate;
				if (expectedVersion != null && trackedAggregate.Version != expectedVersion)
					throw new ConcurrencyException(trackedAggregate.Id);
				return trackedAggregate;
			}

			var aggregate = Repository.Get<TAggregateRoot>(id);
			if (expectedVersion != null && aggregate.Version != expectedVersion)
				throw new ConcurrencyException(id);
			Add(aggregate);

			return aggregate;
		}

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="id">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		public TAggregateRoot GetToVersion<TAggregateRoot>(Guid id, int version)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			var aggregate = Repository.GetToVersion<TAggregateRoot>(id, version);

			return aggregate;
		}

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="id">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		public TAggregateRoot GetToDate<TAggregateRoot>(Guid id, DateTime versionedDate)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
		{
			var aggregate = Repository.GetToDate<TAggregateRoot>(id, versionedDate);

			return aggregate;
		}

		private bool IsTracked(Guid id)
		{
			return TrackedAggregates.ContainsKey(id);
		}

		/// <summary>
		/// Commit any changed <see cref="AggregateRoot{TAuthenticationToken}"/> added to this <see cref="IUnitOfWork{TAuthenticationToken}"/> via <see cref="Add{T}"/>
		/// into the <see cref="IAggregateRepository{TAuthenticationToken}"/>
		/// </summary>
		public void Commit()
		{
			foreach (IAggregateDescriptor<TAuthenticationToken> descriptor in TrackedAggregates.Values)
			{
				Repository.Save(descriptor.Aggregate, descriptor.Version);
			}
			TrackedAggregates.Clear();
		}
	}
}
