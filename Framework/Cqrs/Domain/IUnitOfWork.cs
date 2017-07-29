#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;

namespace Cqrs.Domain
{
	/// <summary>
	/// Provides a basic container to control when <see cref="IEvent{TAuthenticationToken}">events</see> are store in an <see cref="IEventStore{TAuthenticationToken}"/> and then published on an <see cref="IEventPublisher{TAuthenticationToken}"/>.
	/// </summary>
	public interface IUnitOfWork<TAuthenticationToken>
	{
		/// <summary>
		/// Add an item into the <see cref="IUnitOfWork{TAuthenticationToken}"/> ready to be committed.
		/// </summary>
		void Add<TAggregateRoot>(TAggregateRoot aggregate)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> if it has already been loaded.
		/// </summary>
		TAggregateRoot Get<TAggregateRoot>(Guid id, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Commit any changed <see cref="AggregateRoot{TAuthenticationToken}"/> added to this <see cref="IUnitOfWork{TAuthenticationToken}"/> via <see cref="Add{T}"/>
		/// </summary>
		void Commit();
	}
}