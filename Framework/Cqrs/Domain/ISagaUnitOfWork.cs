#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Domain
{
	/// <summary>
	/// This is a Unit of Work for sagas
	/// </summary>
	public interface ISagaUnitOfWork<TAuthenticationToken>
	{
		/// <summary>
		/// Add an item into the <see cref="ISagaUnitOfWork{TAuthenticationToken}"/> ready to be committed.
		/// </summary>
		void Add<TSaga>(TSaga saga)
			where TSaga : ISaga<TAuthenticationToken>;

		/// <summary>
		/// Get an item from the <see cref="ISagaUnitOfWork{TAuthenticationToken}"/> if it has already been loaded.
		/// </summary>
		TSaga Get<TSaga>(Guid id, int? expectedVersion = null)
			where TSaga : ISaga<TAuthenticationToken>;

		/// <summary>
		/// Commit any changed <see cref="Saga{TAuthenticationToken}"/> added to this <see cref="ISagaUnitOfWork{TAuthenticationToken}"/> via <see cref="Add{T}"/>
		/// </summary>
		void Commit();
	}
}