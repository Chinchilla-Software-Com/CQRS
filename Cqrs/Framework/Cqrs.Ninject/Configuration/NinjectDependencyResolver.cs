using System;
using System.Linq;
using Cqrs.Configuration;
using Ninject;
using Ninject.Parameters;

namespace Cqrs.Ninject.Configuration
{
	public class NinjectDependencyResolver : IServiceLocator
	{
		public static IServiceLocator Current { get; protected set; }

		static NinjectDependencyResolver()
		{
			Current = new NinjectDependencyResolver(new StandardKernel());
		}

		protected IKernel Kernel { get; private set; }

		public NinjectDependencyResolver(IKernel kernel)
		{
			Kernel = kernel;
		}

		/// <summary>
		/// Starts the <see cref="NinjectDependencyResolver"/>
		/// </summary>
		/// <remarks>
		/// this exists to the static constructor can be triggered.
		/// </remarks>
		public static void Start()
		{
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