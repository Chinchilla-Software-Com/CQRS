using System;
using System.Collections.Generic;
using Cqrs.Domain.Exception;

namespace Cqrs.Domain
{
	/// <summary>
	/// This is a Unit of Work
	/// </summary>
	public class UnitOfWork : IUnitOfWork
	{
		private IRepository Repository { get; set; }
		private Dictionary<Guid, AggregateDescriptor> TrackedAggregates { get; set; }

		public UnitOfWork(IRepository repository)
		{
			if(repository == null)
				throw new ArgumentNullException("repository");

			Repository = repository;
			TrackedAggregates = new Dictionary<Guid, AggregateDescriptor>();
		}

		/// <summary>
		/// Add an item into the <see cref="IUnitOfWork"/> ready to be committed.
		/// </summary>
		public void Add<TAggregateRoot>(TAggregateRoot aggregate)
			where TAggregateRoot : IAggregateRoot
		{
			if (!IsTracked(aggregate.Id))
				TrackedAggregates.Add(aggregate.Id, new AggregateDescriptor {Aggregate = aggregate, Version = aggregate.Version});
			else if (TrackedAggregates[aggregate.Id].Aggregate != (IAggregateRoot)aggregate)
				throw new ConcurrencyException(aggregate.Id);
		}

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork"/> if it has already been loaded or get it from the <see cref="IRepository"/>.
		/// </summary>
		public TAggregateRoot Get<TAggregateRoot>(Guid id, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot
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

		private bool IsTracked(Guid id)
		{
			return TrackedAggregates.ContainsKey(id);
		}

		/// <summary>
		/// Commit any changed <see cref="AggregateRoot"/> added to this <see cref="IUnitOfWork"/> via <see cref="Add{T}"/>
		/// into the <see cref="IRepository"/>
		/// </summary>
		public void Commit()
		{
			foreach (AggregateDescriptor descriptor in TrackedAggregates.Values)
			{
				Repository.Save(descriptor.Aggregate, descriptor.Version);
			}
			TrackedAggregates.Clear();
		}
	}
}
