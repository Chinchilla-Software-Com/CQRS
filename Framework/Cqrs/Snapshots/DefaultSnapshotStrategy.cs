using System;
using System.Linq;
using Cqrs.Domain;

namespace Cqrs.Snapshots
{
	/// <summary>
	/// An <see cref="ISnapshotStrategy{TAuthenticationToken}"/> that takes a snapshot every 15 versions
	/// </summary>
	/// <typeparam name="TAuthenticationToken"></typeparam>
	public class DefaultSnapshotStrategy<TAuthenticationToken> : ISnapshotStrategy<TAuthenticationToken>
	{
		private const int SnapshotInterval = 15;

		public bool IsSnapshotable(Type aggregateType)
		{
			if (aggregateType.BaseType == null)
				return false;
			if (aggregateType.BaseType.IsGenericType && aggregateType.BaseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<,>))
				return true;
			return IsSnapshotable(aggregateType.BaseType);
		}

		public bool ShouldMakeSnapShot(IAggregateRoot<TAuthenticationToken> aggregate)
		{
			if (!IsSnapshotable(aggregate.GetType()))
				return false;
			int i = aggregate.Version;

			int limit = aggregate.GetUncommittedChanges().Count();
			for (int j = 0; j < limit; j++)
				if (++i % SnapshotInterval == 0 && i != 0)
					return true;
			return false;
		}
	}
}