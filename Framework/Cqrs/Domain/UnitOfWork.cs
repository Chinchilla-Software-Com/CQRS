#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Domain.Exceptions;

namespace Cqrs.Domain
{
	/// <summary>
	/// This is a Unit of Work. This shouldn't normally be used as a singleton.
	/// </summary>
	public class UnitOfWork<TAuthenticationToken> : IUnitOfWork<TAuthenticationToken>
	{
		private IRepository<TAuthenticationToken> Repository { get; set; }

		private Dictionary<Guid, IAggregateDescriptor<TAuthenticationToken>> TrackedAggregates { get; set; }

		public UnitOfWork(IRepository<TAuthenticationToken> repository)
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
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> if it has already been loaded or get it from the <see cref="IRepository{TAuthenticationToken}"/>.
		/// </summary>
		public TAggregateRoot Get<TAggregateRoot>(Guid id, int? expectedVersion = null)
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

		private bool IsTracked(Guid id)
		{
			return TrackedAggregates.ContainsKey(id);
		}

		/// <summary>
		/// Commit any changed <see cref="AggregateRoot{TAuthenticationToken}"/> added to this <see cref="IUnitOfWork{TAuthenticationToken}"/> via <see cref="Add{T}"/>
		/// into the <see cref="IRepository{TAuthenticationToken}"/>
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
