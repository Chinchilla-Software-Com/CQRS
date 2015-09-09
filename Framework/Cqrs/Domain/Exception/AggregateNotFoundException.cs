using System;

namespace Cqrs.Domain.Exception
{
	[Serializable]
	public class AggregateNotFoundException : System.Exception
	{
		public AggregateNotFoundException(Guid id)
			: base(string.Format("Aggregate {0} was not found", id))
		{
		}
	}
}