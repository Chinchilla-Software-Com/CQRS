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

		public virtual TAggregate CreateAggregate<TAggregate>(Guid? rsn = null)
		{
			try
			{
				return DependencyResolver.Resolve<TAggregate>();
			}
			catch (Exception)
			{
				DependencyResolver.Resolve<ILogger>().LogDebug(string.Format("Using the dependency resolver to create an instance of the aggregate typed '{0}' failed.", typeof(TAggregate).FullName), "Cqrs.Domain.Factories.AggregateFactory.CreateAggregate");
				try
				{
					return (TAggregate)Activator.CreateInstance(typeof(TAggregate), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { DependencyResolver, DependencyResolver.Resolve<ILogger>(), rsn }, null);
				}
				catch (MissingMethodException exception)
				{
					DependencyResolver.Resolve<ILogger>().LogDebug(string.Format("Looking for a private constructor with a dependency resolver and logger, to create an instance of the aggregate typed '{0}' failed.", typeof(TAggregate).FullName), "Cqrs.Domain.Factories.AggregateFactory.CreateAggregate", exception);
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