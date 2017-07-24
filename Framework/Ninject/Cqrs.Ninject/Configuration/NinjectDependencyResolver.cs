#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Configuration;
using Ninject;
using Ninject.Modules;
using Ninject.Parameters;
using Ninject.Syntax;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// Provides an ability to resolve instances of objects using Ninject
	/// </summary>
	public class NinjectDependencyResolver : DependencyResolver
	{
		/// <summary>
		/// The inner Ninject <see cref="IKernel"/> used by this instance.
		/// </summary>
		public IKernel Kernel { get; private set; }

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> instances to load when we call <see cref="PrepareKernel"/>
		/// </summary>
		public static IList<INinjectModule> ModulesToLoad = new List<INinjectModule>();

		/// <summary>
		/// A user supplied <see cref="Func{TResult}"/> that will be called during <see cref="Start"/> to create and populate <see cref="DependencyResolver.Current"/>.
		/// </summary>
		public static Func<IKernel, NinjectDependencyResolver> DependencyResolverCreator { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="NinjectDependencyResolver"/>
		/// </summary>
		public NinjectDependencyResolver(IKernel kernel)
		{
			Kernel = kernel;
			BindDependencyResolver();
		}

		/// <summary>
		/// Checks if <see cref="IDependencyResolver"/> has already been registered and if not, registers this instance to it.
		/// </summary>
		protected virtual void BindDependencyResolver()
		{
			bool isDependencyResolverBound = Kernel.GetBindings(typeof(IDependencyResolver)).Any();
			if (!isDependencyResolverBound)
			{
				Kernel.Bind<IDependencyResolver>()
					.ToConstant(this)
					.InSingletonScope();
			}
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

		/// <summary>
		/// Calls <see cref="IKernel.Load(IEnumerable{INinjectModule})"/> passing in <see cref="ModulesToLoad"/>
		/// </summary>
		/// <param name="kernel">The <see cref="IKernel"/> the <see cref="ModulesToLoad"/> will be loaded into.</param>
		public static void PrepareKernel(IKernel kernel)
		{
			kernel.Load
			(
				ModulesToLoad
			);
		}

		/// <summary>
		/// Resolves a single instance for the specified <typeparamref name="T"/>.
		/// by calling <see cref="IResolutionRoot.Resolve"/>
		/// </summary>
		public override T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		/// <summary>
		/// Resolves a single instance for the specified <paramref name="type"/>.
		/// by calling <see cref="IResolutionRoot.Resolve"/>
		/// </summary>
		public override object Resolve(Type type)
		{
			return Kernel.Resolve(Kernel.CreateRequest(type, null, new Parameter[0], true, true)).SingleOrDefault();
		}
	}
}