using System.Collections.Generic;
using Cqrs.Domain;
using Cqrs.Events;

namespace Cqrs.Akka.Domain
{
	public interface IAkkaRepository<TAuthenticationToken> : IRepository<TAuthenticationToken>
	{
		void LoadAggregateHistory<TAggregateRoot>(TAggregateRoot aggregate, IList<IEvent<TAuthenticationToken>> events = null, bool throwExceptionOnNoEvents = true)
			where TAggregateRoot : IAggregateRoot<TAuthenticationToken>;
	}
}