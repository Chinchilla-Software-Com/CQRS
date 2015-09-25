using System;
using Akka.Actor;
using Cqrs.Ninject.Configuration;
using Ninject;

namespace Cqrs.Ninject.Akka
{
	public class AkkaNinjectDependencyResolver : NinjectDependencyResolver
	{
		protected global::Akka.DI.Ninject.NinjectDependencyResolver RawAkkaNinjectDependencyResolver { get; set; }

		public AkkaNinjectDependencyResolver(IKernel kernel, ActorSystem system)
			: base(kernel)
		{
			RawAkkaNinjectDependencyResolver = new global::Akka.DI.Ninject.NinjectDependencyResolver(kernel, system);
		}

		/// <summary>
		/// Starts the <see cref="AkkaNinjectDependencyResolver"/>
		/// </summary>
		/// <remarks>
		/// this exists to the static constructor can be triggered.
		/// </remarks>
		public static void Start(IKernel kernel = null, bool prepareProvidedKernel = false)
		{
			// Create the ActorSystem and Dependency Resolver
			ActorSystem system = ActorSystem.Create("Cqrs");

			DependencyResolverCreator = container => new AkkaNinjectDependencyResolver(container, system);
		}

		#region Overrides of NinjectDependencyResolver

		public override object Resolve(Type serviceType)
		{
			try
			{
				return RawAkkaNinjectDependencyResolver.CreateActorFactory(serviceType)();
			}
			catch
			{
				return base.Resolve(serviceType);
			}
		}

		#endregion
	}
}
