#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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