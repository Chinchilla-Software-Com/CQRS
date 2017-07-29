#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Domain;

namespace Cqrs.Snapshots
{
	/// <summary>
	/// Provides information about the ability to make and get <see cref="Snapshot">snapshots</see>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface ISnapshotStrategy<TAuthenticationToken>
	{
		/// <summary>
		/// Indicates if the provided <paramref name="aggregate"/> should have a <see cref="Snapshot"/> made.
		/// This does NOT indicate if the provided <paramref name="aggregate"/> can have a <see cref="Snapshot"/> made or not.
		/// </summary>
		/// <param name="aggregate">The <see cref="IAggregateRoot{TAuthenticationToken}"/> to check.</param>
		bool ShouldMakeSnapShot(IAggregateRoot<TAuthenticationToken> aggregate);

		/// <summary>
		/// Indicates if the provided <paramref name="aggregateType"/> can have a <see cref="Snapshot"/> made or not.
		/// </summary>
		/// <param name="aggregateType">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> to check.</param>
		bool IsSnapshotable(Type aggregateType);
	}
}