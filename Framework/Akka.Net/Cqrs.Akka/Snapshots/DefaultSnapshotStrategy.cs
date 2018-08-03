#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Domain;
using Cqrs.Snapshots;

namespace Cqrs.Akka.Snapshots
{
	/// <summary>
	/// An <see cref="DefaultSnapshotStrategy{TAuthenticationToken}"/> for Akka.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class DefaultAkkaSnapshotStrategy<TAuthenticationToken> : DefaultSnapshotStrategy<TAuthenticationToken>
	{
		/// <summary>
		/// Indicates if the <paramref name="aggregateType"/> is able to be snapshotted by checking if the <paramref name="aggregateType"/>
		/// directly inherits <see cref="SnapshotAggregateRoot{TAuthenticationToken,TSnapshot}"/>
		/// </summary>
		/// <param name="aggregateType">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> to check.</param>
		public override bool IsSnapshotable(Type aggregateType)
		{
			if (aggregateType.BaseType == null)
				return false;
			if (aggregateType.BaseType.IsGenericType && (aggregateType.BaseType.GetGenericTypeDefinition() == typeof(AkkaSnapshotAggregateRoot<,>) || aggregateType.BaseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<,>)))
				return true;
			return IsSnapshotable(aggregateType.BaseType);
		}
	}
}