using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Akka.Actor;
using Cqrs.Akka.Configuration;
using Cqrs.Akka.Domain;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Ninject.Configuration;
using Ninject;

namespace Cqrs.Ninject.Akka
{
	public class AkkaNinjectDependencyResolver
		: NinjectDependencyResolver
		, IAkkaAggregateResolver
		, IHandlerResolver
	{
		protected global::Akka.DI.Ninject.NinjectDependencyResolver RawAkkaNinjectDependencyResolver { get; set; }

		protected ActorSystem AkkaSystem { get; private set; }

		protected IDictionary<Type, IActorRef> AkkaActors { get; private set; }

		protected IAggregateFactory AggregateFactory { get; private set; }

		public AkkaNinjectDependencyResolver(IKernel kernel, ActorSystem system)
			: base(kernel)
		{
			RawAkkaNinjectDependencyResolver = new global::Akka.DI.Ninject.NinjectDependencyResolver(kernel, AkkaSystem = system);
			AkkaActors = new ConcurrentDictionary<Type, IActorRef>();
			AggregateFactory = Resolve<IAggregateFactory>();
		}

		/// <summary>
		/// Starts the <see cref="AkkaNinjectDependencyResolver"/>
		/// </summary>
		/// <remarks>
		/// this exists to the static constructor can be triggered.
		/// </remarks>
		public new static void Start(IKernel kernel = null, bool prepareProvidedKernel = false)
		{
			// Create the ActorSystem and Dependency Resolver
			ActorSystem system = ActorSystem.Create("Cqrs");

			DependencyResolverCreator = container => new AkkaNinjectDependencyResolver(container, system);
			NinjectDependencyResolver.Start(kernel, prepareProvidedKernel);
		}

		public static void Stop()
		{
			var di = Current as AkkaNinjectDependencyResolver;
			if (di != null)
				di.AkkaSystem.Shutdown();
		}

		#region Overrides of NinjectDependencyResolver

		public override object Resolve(Type serviceType)
		{
			return Resolve(serviceType, null);
		}

		#endregion

		#region Implementation of IAkkaAggregateResolver

		public virtual IActorRef ResolveActor<TAggregate, TAuthenticationToken>(Guid rsn)
			where TAggregate : IAggregateRoot<TAuthenticationToken>
		{
			return (IActorRef)Resolve(typeof(TAggregate), rsn);
		}

		public IActorRef ResolveActor<T>()
		{
			return (IActorRef)Resolve(typeof(T));
		}

		#endregion

		protected virtual object RootResolve(Type serviceType)
		{
			return base.Resolve(serviceType);
		}

		public virtual object Resolve(Type serviceType, object rsn)
		{
			IActorRef actorReference;
			try
			{
				if (AkkaActors.TryGetValue(serviceType, out actorReference))
					return actorReference;

				return base.Resolve(serviceType);
			}
			catch (/*ActorInitialization*/Exception)
			{
				Props properties;
				Type typeToTest = serviceType;
				while (typeToTest != null)
				{
					Type[] types = typeToTest.GenericTypeArguments;
					if (types.Length == 1)
					{
						Type aggregateType = typeof (AkkaAggregateRoot<>).MakeGenericType(typeToTest.GenericTypeArguments.Single());
						if (typeToTest == aggregateType)
						{
							typeToTest = aggregateType;
							break;
						}
					}
					typeToTest = typeToTest.BaseType;
				}
				if (typeToTest == null || !(typeToTest).IsAssignableFrom(serviceType))
					properties = Props.Create(() => (ActorBase)RootResolve(serviceType));
				else
					properties = Props.Create(() => (ActorBase) AggregateFactory.CreateAggregate(serviceType, rsn as Guid?, false));
				string actorName = serviceType.FullName.Replace("`", string.Empty);
				int index = actorName.IndexOf("[[", StringComparison.Ordinal);
				if (index > -1)
					actorName = actorName.Substring(0, index);
				actorReference = AkkaSystem.ActorOf(properties, string.Format("{0}~{1}", actorName, rsn));
				AkkaActors.Add(serviceType, actorReference);
				return actorReference;
			}
		}
	}
}