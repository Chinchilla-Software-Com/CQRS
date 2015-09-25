using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Configuration;
using Ninject;
using Ninject.Modules;
using Ninject.Parameters;

namespace Cqrs.Ninject.Configuration
{
	public class NinjectDependencyResolver : IDependencyResolver
	{
		public static IDependencyResolver Current { get; protected set; }

		protected IKernel Kernel { get; private set; }

		public static IList<INinjectModule> ModulesToLoad = new List<INinjectModule>();

		public static Func<IKernel, NinjectDependencyResolver> DependencyResolverCreator { get; set; }

		public NinjectDependencyResolver(IKernel kernel)
		{
			Kernel = kernel;

			Kernel.Bind<IDependencyResolver>()
				.ToConstant(this)
				.InSingletonScope();
		}

		/// <summary>
		/// Starts the <see cref="NinjectDependencyResolver"/>
		/// </summary>
		/// <remarks>
		/// this exists to the static constructor can be triggered.
		/// </remarks>
		public static void Start(IKernel kernel = null, bool prepareProvidedKernel = false)
		{
			if (kernel == null)
			{
				kernel = new StandardKernel();
				prepareProvidedKernel = true;
			}

			if (DependencyResolverCreator != null)
				Current = DependencyResolverCreator(kernel);
			else
				Current = new NinjectDependencyResolver(kernel);

			if (prepareProvidedKernel)
				PrepareKernel(kernel);
		}

		public static void PrepareKernel(IKernel kernel)
		{
			kernel.Load
			(
				ModulesToLoad
			);
		}

		public virtual T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		public virtual object Resolve(Type serviceType)
		{
			return Kernel.Resolve(Kernel.CreateRequest(serviceType, null, new Parameter[0], true, true)).SingleOrDefault();
		}
	}
}