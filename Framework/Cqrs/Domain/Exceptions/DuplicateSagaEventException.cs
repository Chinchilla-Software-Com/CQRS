using System;

namespace Cqrs.Domain.Exceptions
{
	[Serializable]
	public class DuplicateSagaEventException<TSaga, TAuthenticationToken> : Exception
		where TSaga : ISaga<TAuthenticationToken>
	{
		public DuplicateSagaEventException(Guid id, int version)
			: base(string.Format("Eventstore gave more than one event for saga '{0}' of type '{1}' for version {2}", id, typeof(TSaga).FullName, version))
		{
		}
	}
}