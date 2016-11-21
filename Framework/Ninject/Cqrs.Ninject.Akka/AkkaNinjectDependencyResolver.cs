using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using Akka.DI.Core;
using Cqrs.Akka.Configuration;
using Cqrs.Akka.Domain;
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

		protected ActorSystem AkkaSystem { get; set; }

		protected IDictionary<Type, IActorRef> AkkaActors { get; set; }

		public AkkaNinjectDependencyResolver(IKernel kernel, ActorSystem system)
			: base(kernel)
		{
			RawAkkaNinjectDependencyResolver = new global::Akka.DI.Ninject.NinjectDependencyResolver(kernel, AkkaSystem = system);
			AkkaActors = new ConcurrentDictionary<Type, IActorRef>();
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
			catch (ActorInitializationException)
			{
				actorReference = AkkaSystem.ActorOf(AkkaSystem.GetExtension<DIExt>().Props(serviceType), serviceType.FullName);
				AkkaActors.Add(serviceType, actorReference);
				return actorReference;
			}
		}
	}
}
