using System;
using System.Reflection;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Domain.Exception;

namespace Cqrs.Domain.Factories
{
	public class AggregateFactory : IAggregateFactory
	{
		protected IDependencyResolver DependencyResolver { get; private set; }

		public AggregateFactory(IDependencyResolver dependencyResolver)
		{
			DependencyResolver = dependencyResolver;
		}

		public TAggregate CreateAggregate<TAggregate>()
		{
			try
			{
				return DependencyResolver.Resolve<TAggregate>();
			}
			catch (System.Exception)
			{
				DependencyResolver.Resolve<ILogger>().LogInfo("Using the dependency resolver to create an instance of the aggregate failed.", "Cqrs.Domain.Factories.AggregateFactory.CreateAggregate");
				try
				{
					return (TAggregate)Activator.CreateInstance(typeof(TAggregate), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { DependencyResolver, DependencyResolver.Resolve<ILogger>() }, null);
				}
				catch (MissingMethodException exception)
				{
					DependencyResolver.Resolve<ILogger>().LogDebug("Looking for a private constructor with a dependency resolver and logger, to create an instance of the aggregate failed.", "Cqrs.Domain.Factories.AggregateFactory.CreateAggregate", exception);
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
	}
}