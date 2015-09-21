using System;

namespace Cqrs.Domain.Exception
{
	[Serializable]
	public class DuplicateEventException<TAggregateRoot, TAuthenticationToken> : System.Exception
		where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		public DuplicateEventException(Guid id, int version)
			: base(string.Format("Eventstore gave more than one event for aggregate '{0}' of type '{1}' for version {2}", id, typeof(TAggregateRoot).FullName, version))
		{
		}
	}
}