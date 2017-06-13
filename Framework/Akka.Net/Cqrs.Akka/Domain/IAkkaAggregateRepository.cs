using System.Collections.Generic;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Akka.Domain
{
	public interface IAkkaAggregateRepository<TAuthenticationToken> : IAggregateRepository<TAuthenticationToken>
	{
		void LoadAggregateHistory<TAggregateRoot>(TAggregateRoot aggregate, IList<IEvent<TAuthenticationToken>> events = null, bool throwExceptionOnNoEvents = true)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;
	}
}