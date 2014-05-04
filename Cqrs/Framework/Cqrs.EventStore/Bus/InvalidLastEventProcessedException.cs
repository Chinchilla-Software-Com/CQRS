using System;
using System.Runtime.Serialization;

namespace Cqrs.EventStore.Bus
{
	[Serializable]
	public class InvalidLastEventProcessedException : Exception
	{
		public InvalidLastEventProcessedException()
		{
		}

		public InvalidLastEventProcessedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public InvalidLastEventProcessedException(string message)
			: base(message)
		{
		}

		protected InvalidLastEventProcessedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}