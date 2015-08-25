using System;

namespace Cqrs.Domain.Exception
{
	[Serializable]
	public class DuplicateEventException : System.Exception
	{
		public DuplicateEventException(Guid id, int version)
			: base(string.Format("Eventstore gave more than one event for aggregate {0} for version {1}", id, version))
		{
		}
	}
}