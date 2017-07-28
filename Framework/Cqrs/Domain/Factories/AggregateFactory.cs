#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Reflection;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Domain.Exceptions;

namespace Cqrs.Domain.Factories
{
	/// <summary>
	/// A factory for creating instances of aggregates.
	/// </summary>
	public class AggregateFactory : IAggregateFactory
	{
		/// <summary>
		/// Gets or sets the <see cref="IDependencyResolver"/> used.
		/// </summary>
		protected IDependencyResolver DependencyResolver { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/> used.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AggregateFactory"/>.
		/// </summary>
		public AggregateFactory(IDependencyResolver dependencyResolver, ILogger logger)
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		/// <summary>
		/// Creates instance of <typeparamref name="TAggregate"/>.
		/// </summary>
		/// <typeparam name="TAggregate">The <see cref="Type"/> of the aggregate to create.</typeparam>
		/// <param name="rsn">The identifier of the aggregate to create an instance of if an existing aggregate.</param>
		/// <param name="tryDependencyResolutionFirst">Indicates the use of <see cref="IDependencyResolver"/> should be tried first.</param>
		public virtual TAggregate Create<TAggregate>(Guid? rsn = null, bool tryDependencyResolutionFirst = true)
		{
			return (TAggregate)Create(typeof (TAggregate), rsn);
		}

		/// <summary>
		/// Creates instance of type <paramref name="aggregateType"/>
		/// </summary>
		/// <param name="aggregateType">The <see cref="Type"/> of the aggregate to create.</param>
		/// <param name="rsn">The identifier of the aggregate to create an instance of if an existing aggregate.</param>
		/// <param name="tryDependencyResolutionFirst">Indicates the use of <see cref="IDependencyResolver"/> should be tried first.</param>
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