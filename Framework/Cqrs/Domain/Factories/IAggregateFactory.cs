using System;
using Cqrs.Configuration;

namespace Cqrs.Domain.Factories
{
	/// <summary>
	/// A factory for creating instances of aggregates.
	/// </summary>
	public interface IAggregateFactory
	{
		/// <summary>
		/// Creates instance of <typeparamref name="TAggregate"/>.
		/// </summary>
		/// <typeparam name="TAggregate">The <see cref="Type"/> of the aggregate to create.</typeparam>
		/// <param name="rsn">The identifier of the aggregate to create an instance of if an existing aggregate.</param>
		/// <param name="tryDependencyResolutionFirst">Indicates the use of <see cref="IDependencyResolver"/> should be tried first.</param>
		TAggregate Create<TAggregate>(Guid? rsn = null, bool tryDependencyResolutionFirst = true);

		/// <summary>
		/// Creates instance of type <paramref name="aggregateType"/>
		/// </summary>
		/// <param name="aggregateType">The <see cref="Type"/> of the aggregate to create.</param>
		/// <param name="rsn">The identifier of the aggregate to create an instance of if an existing aggregate.</param>
		/// <param name="tryDependencyResolutionFirst">Indicates the use of <see cref="IDependencyResolver"/> should be tried first.</param>
		object Create(Type aggregateType, Guid? rsn = null, bool tryDependencyResolutionFirst = true);
	}
}