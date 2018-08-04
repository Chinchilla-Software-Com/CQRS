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
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		void Add<TAggregateRoot>(TAggregateRoot aggregate, bool useSnapshots = false)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> if it has already been loaded.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		TAggregateRoot Get<TAggregateRoot>(Guid id, int? expectedVersion = null, bool useSnapshots = false)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> up to and including the provided <paramref name="version"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="id">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="version">Load events up-to and including from this version</param>
		TAggregateRoot GetToVersion<TAggregateRoot>(Guid id, int version)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork{TAuthenticationToken}"/> up to and including the provided <paramref name="versionedDate"/>.
		/// </summary>
		/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> the <see cref="IEvent{TAuthenticationToken}"/> was raised in.</typeparam>
		/// <param name="id">The <see cref="IAggregateRoot{TAuthenticationToken}.Id"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/>.</param>
		/// <param name="versionedDate">Load events up-to and including from this <see cref="DateTime"/></param>
		TAggregateRoot GetToDate<TAggregateRoot>(Guid id, DateTime versionedDate)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Commit any changed <see cref="AggregateRoot{TAuthenticationToken}"/> added to this <see cref="IUnitOfWork{TAuthenticationToken}"/> via <see cref="Add{T}"/>
		/// </summary>
		void Commit();
	}
}