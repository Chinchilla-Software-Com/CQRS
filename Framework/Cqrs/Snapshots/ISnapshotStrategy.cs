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
	public interface ISnapshotStrategy<TAuthenticationToken>
	{
		bool ShouldMakeSnapShot(IAggregateRoot<TAuthenticationToken> aggregate);

		bool IsSnapshotable(Type aggregateType);
	}
}