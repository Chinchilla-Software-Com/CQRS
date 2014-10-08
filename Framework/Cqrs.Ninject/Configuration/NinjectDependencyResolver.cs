using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Configuration;
using Ninject;
using Ninject.Modules;
using Ninject.Parameters;

namespace Cqrs.Ninject.Configuration
{
	public class NinjectDependencyResolver : IServiceLocator
	{
		public static IServiceLocator Current { get; protected set; }

		protected IKernel Kernel { get; private set; }

		public static IList<INinjectModule> ModulesToLoad = new List<INinjectModule>();

		public NinjectDependencyResolver(IKernel kernel)
		{
			Kernel = kernel;

			Kernel.Bind<IServiceLocator>()
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
				PrepareKernel(kernel);
			}

			if (prepareProvidedKernel)
				PrepareKernel(kernel);

			Current = new NinjectDependencyResolver(kernel);
		}

		public static void PrepareKernel(IKernel kernel)
		{
			kernel.Load
			(
				ModulesToLoad
			);
		}

		public T GetService<T>()
		{
			return Resolve<T>();
		}

		public object GetService(Type type)
		{
			return Resolve(type);
		}

		protected T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		protected object Resolve(Type serviceType)
		{
			return Kernel.Resolve(Kernel.CreateRequest(serviceType, null, new Parameter[0], true, true)).SingleOrDefault();
		}
	}
}