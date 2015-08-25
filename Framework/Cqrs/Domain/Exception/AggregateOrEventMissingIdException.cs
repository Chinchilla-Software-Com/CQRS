using System;

namespace Cqrs.Domain.Exception
{
	[Serializable]
	public class AggregateOrEventMissingIdException : System.Exception
	{
		public AggregateOrEventMissingIdException(Type aggregateType, Type eventType)
			: base(string.Format("An event of type {0} tried to be saved from {1} but no id was set on either", eventType.FullName, aggregateType.FullName))
		{
		}
	}
}