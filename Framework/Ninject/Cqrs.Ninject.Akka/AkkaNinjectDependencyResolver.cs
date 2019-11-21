#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Chinchilla.Logging;
using Cqrs.Akka.Configuration;
using Cqrs.Akka.Domain;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Ninject.Configuration;
using Ninject;
using Ninject.Activation;

namespace Cqrs.Ninject.Akka
{
	/// <summary>
	/// Provides an ability to resolve instances of objects and Akka.NET objects using Ninject
	/// </summary>
	public class AkkaNinjectDependencyResolver
		: NinjectDependencyResolver
		, IAkkaAggregateResolver
		, IAkkaSagaResolver
		, IHandlerResolver
	{
		/// <summary>
		/// The inner resolver used by Akka.NET
		/// </summary>
		protected global::Akka.DI.Ninject.NinjectDependencyResolver RawAkkaNinjectDependencyResolver { get; set; }

		/// <summary>
		/// The <see cref="ActorSystem"/> as part of Akka.NET.
		/// </summary>
		protected ActorSystem AkkaSystem { get; private set; }

		/// <summary>
		/// A generic type, quick reference, lookup for fast resolving of Akka.NET objects since the patterns calls for them to be treated like statics
		/// </summary>
		protected IDictionary<Type, IActorRef> AkkaActors { get; private set; }

		/// <summary>
		/// The <see cref="IAggregateFactory"/> that will be used to create new instances of Akka.NET objects.
		/// </summary>
		protected IAggregateFactory AggregateFactory { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaNinjectDependencyResolver"/>
		/// </summary>
		public AkkaNinjectDependencyResolver(IKernel kernel, ActorSystem system)
			: base(kernel)
		{
			RawAkkaNinjectDependencyResolver = new global::Akka.DI.Ninject.NinjectDependencyResolver(kernel, AkkaSystem = system);
			AkkaActors = new ConcurrentDictionary<Type, IActorRef>();
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			AggregateFactory = Resolve<IAggregateFactory>();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		/// <summary>
		/// Checks if an instance of <see cref="IDependencyResolver"/> is already registered, if one is registered, it in unregistered and this instance is registered as the <see cref="IDependencyResolver"/>.
		/// It then checks if an instance of <see cref="IAkkaAggregateResolver"/> is already registered, if one is registered, it in unregistered and this instance is registered as the <see cref="IAkkaAggregateResolver"/>
		/// </summary>
		protected override void BindDependencyResolver()
		{
			bool isDependencyResolverBound = Kernel.GetBindings(typeof(IDependencyResolver)).Any();
			if (isDependencyResolverBound)
				Kernel.Unbind<IDependencyResolver>();
			Kernel.Bind<IDependencyResolver>()
				.ToConstant(this)
				.InSingletonScope();

			isDependencyResolverBound = Kernel.GetBindings(typeof(IAkkaAggregateResolver)).Any();
			if (!isDependencyResolverBound)
			{
				Kernel.Bind<IAkkaAggregateResolver>()
					.ToConstant(this)
					.InSingletonScope();
			}
		}

		/// <summary>
		/// Starts the <see cref="AkkaNinjectDependencyResolver"/>
		/// </summary>
		/// <remarks>
		/// This exists so the static constructor can be triggered.
		/// </remarks>
		public new static void Start(IKernel kernel = null, bool prepareProvidedKernel = false)
		{
			// Create the ActorSystem and Dependency Resolver
			ActorSystem system = ActorSystem.Create("Cqrs");

			Func<IKernel, NinjectDependencyResolver> originalDependencyResolverCreator = DependencyResolverCreator;
			Func<IKernel, NinjectDependencyResolver> dependencyResolverCreator = container => new AkkaNinjectDependencyResolver(container, system);
			if (originalDependencyResolverCreator == null)
				DependencyResolverCreator = dependencyResolverCreator;
			else
				DependencyResolverCreator = container =>
				{
					originalDependencyResolverCreator(container);
					return dependencyResolverCreator(container);
				};

			NinjectDependencyResolver.Start(kernel, prepareProvidedKernel);

			// Setup an actor that will handle deadletter type messages
			var deadletterWatchMonitorProps = Props.Create(() => new DeadletterToLoggerProxy(Current.Resolve<ILogger>()));
			var deadletterWatchActorRef = system.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor");

			// subscribe to the event stream for messages of type "DeadLetter"
			system.EventStream.Subscribe(deadletterWatchActorRef, typeof(DeadLetter));

		}

		/// <summary>
		/// Calls <see cref="ActorSystem.Shutdown"/>
		/// </summary>
		public static void Stop()
		{
			var di = Current as AkkaNinjectDependencyResolver;
			if (di != null)
				di.AkkaSystem.Shutdown();
		}

		#region Overrides of NinjectDependencyResolver

		/// <summary>
		/// Resolves instances of <paramref name="serviceType"/> using <see cref="Resolve(System.Type, Object)"/>.
		/// </summary>
		public override object Resolve(Type serviceType)
		{
			return Resolve(serviceType, null);
		}

		#endregion

		#region Implementation of IAkkaAggregateResolver

		/// <summary>
		/// Resolves instances of <typeparamref name="TAggregate"/> using <see cref="AkkaResolve"/>.
		/// </summary>
		public virtual IActorRef ResolveActor<TAggregate, TAuthenticationToken>(Guid rsn)
			where TAggregate : IAggregateRoot<TAuthenticationToken>
		{
			return (IActorRef)AkkaResolve(typeof(TAggregate), rsn, true);
		}

		/// <summary>
		/// Resolves instances of <typeparamref name="T"/> using <see cref="AkkaResolve"/>.
		/// </summary>
		public IActorRef ResolveActor<T>()
		{
			return (IActorRef)AkkaResolve(typeof(T), null, true);
		}

		#endregion

		#region Implementation of IAkkaSagaResolver

		/// <summary>
		/// Resolves instances of <typeparamref name="TSaga"/> using <see cref="ResolveSagaActor{TSaga,TAuthenticationToken}"/>.
		/// </summary>
		IActorRef IAkkaSagaResolver.ResolveActor<TSaga, TAuthenticationToken>(Guid rsn)
		{
			return ResolveSagaActor<TSaga, TAuthenticationToken>(rsn);
		}

		/// <summary>
		/// Resolves instances of <typeparamref name="TSaga"/> using <see cref="AkkaResolve"/>.
		/// </summary>
		public virtual IActorRef ResolveSagaActor<TSaga, TAuthenticationToken>(Guid rsn)
			where TSaga : ISaga<TAuthenticationToken>
		{
			return (IActorRef)AkkaResolve(typeof(TSaga), rsn, true);
		}

		#endregion

		/// <summary>
		/// Resolves instances of <paramref name="serviceType"/> using <see cref="IDependencyResolver.Resolve{T}"/>.
		/// </summary>
		protected virtual object RootResolve(Type serviceType)
		{
			return base.Resolve(serviceType);
		}

		/// <summary>
		/// Resolves instances of <paramref name="serviceType"/> using <see cref="AkkaResolve"/>.
		/// </summary>
		public virtual object Resolve(Type serviceType, object rsn)
		{
			return AkkaResolve(serviceType, rsn);
		}

		/// <summary>
		/// Resolves instances of <paramref name="serviceType"/> looking up <see cref="AkkaActors"/>, then <see cref="IDependencyResolver.Resolve{T}"/> and finally <see cref="AggregateFactory"/>.
		/// </summary>
		public virtual object AkkaResolve(Type serviceType, object rsn, bool isAForcedActorSearch = false)
		{
			do
			{
				IActorRef actorReference;
				try
				{
					if (AkkaActors.TryGetValue(serviceType, out actorReference))
						return actorReference;
					if (!isAForcedActorSearch)
						return base.Resolve(serviceType);
				}
				catch (ActivationException) { throw; }
				catch ( /*ActorInitialization*/Exception) { /* */ }

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
						Type sagaType = typeof (AkkaSaga<>).MakeGenericType(typeToTest.GenericTypeArguments.Single());
						if (typeToTest == sagaType)
						{
							typeToTest = sagaType;
							break;
						}
					}
					typeToTest = typeToTest.BaseType;
				}

				// This sorts out an out-of-order binder issue
				if (AggregateFactory == null)
					AggregateFactory = Resolve<IAggregateFactory>();

				if (typeToTest == null || !(typeToTest).IsAssignableFrom(serviceType))
					properties = Props.Create(() => (ActorBase)RootResolve(serviceType));
				else
					properties = Props.Create(() => (ActorBase) AggregateFactory.Create(serviceType, rsn as Guid?, false));
				string actorName = serviceType.FullName.Replace("`", string.Empty);
				int index = actorName.IndexOf("[[", StringComparison.Ordinal);
				if (index > -1)
					actorName = actorName.Substring(0, index);
				try
				{
					actorReference = AkkaSystem.ActorOf(properties, string.Format("{0}~{1}", actorName, rsn));
				}
				catch (InvalidActorNameException)
				{
					// This means that the actor has been created since we tried to get it... funnily enough concurrency doesn't actually mean concurrency.
					continue;
				}
				AkkaActors.Add(serviceType, actorReference);
				return actorReference;
			} while (true);
		}
	}
}