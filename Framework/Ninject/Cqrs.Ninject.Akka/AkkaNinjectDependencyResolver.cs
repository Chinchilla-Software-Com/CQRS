using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using Cqrs.Akka.Configuration;
using Cqrs.Akka.Domain;
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

		#region Overrides of NinjectDependencyResolver

		public override object Resolve(Type serviceType)
		{
			return Resolve(serviceType, null);
		}

		#endregion

		#region Implementation of IAkkaAggregateResolver

		public virtual IActorRef Resolve<TAggregate>(Guid rsn)
		{
			return (IActorRef)Resolve(typeof(TAggregate), rsn);
		}

		#endregion

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
				var properties = Props.Create(() => (ActorBase)AggregateFactory.CreateAggregate(serviceType, rsn as Guid?, false));
				actorReference = AkkaSystem.ActorOf(properties, string.Format("{0}~{1}", serviceType.FullName, rsn));
				AkkaActors.Add(serviceType, actorReference);
				return actorReference;
			}
		}
	}
}