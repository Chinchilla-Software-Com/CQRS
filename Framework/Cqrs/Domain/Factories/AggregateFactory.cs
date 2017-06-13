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

		protected ILogger Logger { get; private set; }

		public AggregateFactory(IDependencyResolver dependencyResolver, ILogger logger)
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		public virtual TAggregate Create<TAggregate>(Guid? rsn = null, bool tryDependencyResolutionFirst = true)
		{
			return (TAggregate)Create(typeof (TAggregate), rsn);
		}

		public object Create(Type aggregateType, Guid? rsn = null, bool tryDependencyResolutionFirst = true)
		{
			if (tryDependencyResolutionFirst)
			{ 
				try
				{
					return DependencyResolver.Resolve(aggregateType);
				}
				catch
				{
					Logger.LogDebug(string.Format("Using the dependency resolver to create an instance of the aggregate typed '{0}' failed.", aggregateType.FullName), "Cqrs.Domain.Factories.AggregateFactory.Create");
				}
			}

			try
			{
				return Activator.CreateInstance(aggregateType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { DependencyResolver, Logger, rsn }, null);
			}
			catch (MissingMethodException exception)
			{
				Logger.LogDebug(string.Format("Looking for a private constructor with a dependency resolver and logger, to create an instance of the aggregate typed '{0}' failed.", aggregateType.FullName), "Cqrs.Domain.Factories.AggregateFactory.Create", exception);
				try
				{
					return Activator.CreateInstance(aggregateType, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { DependencyResolver, Logger }, null);
				}
				catch (MissingMethodException exception2)
				{
					Logger.LogDebug(string.Format("Looking for a private constructor with a dependency resolver and logger, to create an instance of the aggregate typed '{0}' failed.", aggregateType.FullName), "Cqrs.Domain.Factories.AggregateFactory.Create", exception2);
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