using System;
using System.Reflection;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Domain.Exceptions;

namespace Cqrs.Domain.Factories
{
	public class AggregateFactory : IAggregateFactory
	{
		protected IDependencyResolver DependencyResolver { get; private set; }

		public AggregateFactory(IDependencyResolver dependencyResolver)
		{
			DependencyResolver = dependencyResolver;
		}

		public virtual TAggregate CreateAggregate<TAggregate>(Guid? rsn = null, bool tryDependencyResolutionFirst = true)
		{
			return (TAggregate)CreateAggregate(typeof (TAggregate), rsn);
		}

		public object CreateAggregate(Type aggregateType, Guid? rsn = null, bool tryDependencyResolutionFirst = true)
		{
			ILogger logger = DependencyResolver.Resolve<ILogger>();
			if (tryDependencyResolutionFirst)
			{ 
				try
				{
					return DependencyResolver.Resolve(aggregateType);
				}
				catch
				{
					logger.LogDebug(string.Format("Using the dependency resolver to create an instance of the aggregate typed '{0}' failed.", aggregateType.FullName), "Cqrs.Domain.Factories.AggregateFactory.CreateAggregate");
				}
			}

			try
			{
				return Activator.CreateInstance(aggregateType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { DependencyResolver, logger, rsn }, null);
			}
			catch (MissingMethodException exception)
			{
				logger.LogDebug(string.Format("Looking for a private constructor with a dependency resolver and logger, to create an instance of the aggregate typed '{0}' failed.", aggregateType.FullName), "Cqrs.Domain.Factories.AggregateFactory.CreateAggregate", exception);
				try
				{
					return Activator.CreateInstance(aggregateType, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { DependencyResolver, logger }, null);
				}
				catch (MissingMethodException exception2)
				{
					logger.LogDebug(string.Format("Looking for a private constructor with a dependency resolver and logger, to create an instance of the aggregate typed '{0}' failed.", aggregateType.FullName), "Cqrs.Domain.Factories.AggregateFactory.CreateAggregate", exception2);
					try
					{
						return Activator.CreateInstance(aggregateType, true);
					}
					catch (MissingMethodException)
					{
						throw new MissingParameterLessConstructorException(aggregateType);
					}
				}
			}
		}
	}
}