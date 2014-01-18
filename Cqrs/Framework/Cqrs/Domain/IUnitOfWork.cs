using System;

namespace Cqrs.Domain
{
	/// <summary>
	/// This is a Unit of Work
	/// </summary>
	public interface IUnitOfWork<TAuthenticationToken>
	{
		/// <summary>
		/// Add an item into the <see cref="IUnitOfWork"/> ready to be committed.
		/// </summary>
		void Add<TAggregateRoot>(TAggregateRoot aggregate)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Get an item from the <see cref="IUnitOfWork"/> if it has already been loaded.
		/// </summary>
		TAggregateRoot Get<TAggregateRoot>(Guid id, int? expectedVersion = null)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;

		/// <summary>
		/// Commit any changed <see cref="AggregateRoot"/> added to this <see cref="IUnitOfWork"/> via <see cref="Add{T}"/>
		/// </summary>
		void Commit();
	}
}