using System;

namespace Cqrs.Domain.Exception
{
	[Serializable]
	public class EventsOutOfOrderException : System.Exception
	{
		public EventsOutOfOrderException(Guid id, Type aggregateRootType, int currentVersion, int providedEventVersion)
			: base(string.Format("Eventstore gave event for aggregate '{0}' of type '{1}' out of order at version {2} by providing {3}", id, aggregateRootType.FullName, currentVersion, providedEventVersion))
		{
		}
	}
}