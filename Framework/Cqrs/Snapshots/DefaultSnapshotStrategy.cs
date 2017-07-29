#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Domain;

namespace Cqrs.Snapshots
{
	/// <summary>
	/// An <see cref="ISnapshotStrategy{TAuthenticationToken}"/> that takes a snapshot every 15 versions.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class DefaultSnapshotStrategy<TAuthenticationToken> : ISnapshotStrategy<TAuthenticationToken>
	{
		private const int SnapshotInterval = 15;

		/// <summary>
		/// Indicates if the <paramref name="aggregateType"/> is able to be snapshotted by checking if the <paramref name="aggregateType"/>
		/// directly inherits <see cref="SnapshotAggregateRoot{TAuthenticationToken,TSnapshot}"/>
		/// </summary>
		/// <param name="aggregateType">The <see cref="Type"/> of <see cref="IAggregateRoot{TAuthenticationToken}"/> to check.</param>
		public virtual bool IsSnapshotable(Type aggregateType)
		{
			if (aggregateType.BaseType == null)
				return false;
			if (aggregateType.BaseType.IsGenericType && aggregateType.BaseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<,>))
				return true;
			return IsSnapshotable(aggregateType.BaseType);
		}

		/// <summary>
		/// Checks <see cref="IsSnapshotable"/> and if it is, also checks if the calculated version number would be exactly dividable by <see cref="GetSnapshotInterval"/>.
		/// </summary>
		/// <param name="aggregate">The <see cref="IAggregateRoot{TAuthenticationToken}"/> to check.</param>
		public virtual bool ShouldMakeSnapShot(IAggregateRoot<TAuthenticationToken> aggregate)
		{
			if (!IsSnapshotable(aggregate.GetType()))
				return false;

			// Why isn't this something as simple as `(aggregate.Version + aggregate.GetUncommittedChanges().Count()) % SnapshotInterval`???
			int i = aggregate.Version;

			int limit = aggregate.GetUncommittedChanges().Count();
			for (int j = 0; j < limit; j++)
				if (++i % GetSnapshotInterval() == 0 && i != 0)
					return true;
			return false;
		}

		/// <summary>
		/// Returns the value of <see cref="SnapshotInterval"/>.
		/// </summary>
		protected virtual int GetSnapshotInterval()
		{
			return SnapshotInterval;
		}
	}
}