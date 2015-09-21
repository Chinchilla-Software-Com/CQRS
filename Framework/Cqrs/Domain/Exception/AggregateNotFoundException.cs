using System;

namespace Cqrs.Domain.Exception
{
	[Serializable]
	public class AggregateNotFoundException<TAggregateRoot, TAuthenticationToken> : System.Exception
		where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		public AggregateNotFoundException(Guid id)
			: base(string.Format("Aggregate '{0}' of type '{1}' was not found", id, typeof(TAggregateRoot).FullName))
		{
		}
	}
}