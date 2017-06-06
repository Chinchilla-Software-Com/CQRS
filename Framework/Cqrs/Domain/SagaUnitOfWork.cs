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
	public class SagaUnitOfWork<TAuthenticationToken> : ISagaUnitOfWork<TAuthenticationToken>
	{
		private ISagaRepository<TAuthenticationToken> Repository { get; set; }

		private Dictionary<Guid, ISagaDescriptor<TAuthenticationToken>> TrackedSagas { get; set; }

		public SagaUnitOfWork(ISagaRepository<TAuthenticationToken> repository)
		{
			if(repository == null)
				throw new ArgumentNullException("repository");

			Repository = repository;
			TrackedSagas = new Dictionary<Guid, ISagaDescriptor<TAuthenticationToken>>();
		}

		/// <summary>
		/// Add an item into the <see cref="IUnitOfWork{TAuthenticationToken}"/> ready to be committed.
		/// </summary>
		public void Add<TSaga>(TSaga saga)
			where TSaga : ISaga<TAuthenticationToken>
		{
			if (!IsTracked(saga.Id))
			{
				var sagaDescriptor = new SagaDescriptor<TSaga, TAuthenticationToken>
				{
					Saga = saga,
					Version = saga.Version
				};
				TrackedSagas.Add(saga.Id, sagaDescriptor);
			}
			else if (((TrackedSagas[saga.Id]).Saga) != (ISaga<TAuthenticationToken>)saga)
				throw new ConcurrencyException(saga.Id);
		}

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> if it has already been loaded or get it from the <see cref="IRepository{TAuthenticationToken}"/>.
		/// </summary>
		public TSaga Get<TSaga>(Guid id, int? expectedVersion = null)
			where TSaga : ISaga<TAuthenticationToken>
		{
			if(IsTracked(id))
			{
				var trackedSaga = (TSaga)TrackedSagas[id].Saga;
				if (expectedVersion != null && trackedSaga.Version != expectedVersion)
					throw new ConcurrencyException(trackedSaga.Id);
				return trackedSaga;
			}

			var saga = Repository.Get<TSaga>(id);
			if (expectedVersion != null && saga.Version != expectedVersion)
				throw new ConcurrencyException(id);
			Add(saga);

			return saga;
		}

		private bool IsTracked(Guid id)
		{
			return TrackedSagas.ContainsKey(id);
		}

		/// <summary>
		/// Commit any changed <see cref="Saga{TAuthenticationToken}"/> added to this <see cref="IUnitOfWork{TAuthenticationToken}"/> via <see cref="Add{T}"/>
		/// into the <see cref="IRepository{TAuthenticationToken}"/>
		/// </summary>
		public void Commit()
		{
			foreach (ISagaDescriptor<TAuthenticationToken> descriptor in TrackedSagas.Values)
			{
				Repository.Save(descriptor.Saga, descriptor.Version);
			}
			TrackedSagas.Clear();
		}
	}
}
