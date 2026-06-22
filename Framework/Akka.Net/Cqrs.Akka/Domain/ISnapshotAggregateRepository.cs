#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Domain;

namespace Cqrs.Akka.Domain
{
	/// <summary>
	/// Provides basic snapshot repository methods for operations with instances of <see cref="IAggregateRoot{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public interface IAkkaSnapshotAggregateRepository<TAuthenticationToken>
		: ISnapshotAggregateRepository<TAuthenticationToken>
		, IAkkaAggregateRepository<TAuthenticationToken>
	{
	}
}