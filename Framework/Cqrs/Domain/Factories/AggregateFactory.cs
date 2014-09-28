using System;
using Cqrs.Domain.Exception;

namespace Cqrs.Domain.Factories
{
	public class AggregateFactory : IAggregateFactory
	{
		public TAggregate CreateAggregate<TAggregate>()
		{
			try
			{
				return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);
			}
			catch (MissingMethodException)
			{
				throw new MissingParameterLessConstructorException(typeof(TAggregate));
			}
		}
	}
}