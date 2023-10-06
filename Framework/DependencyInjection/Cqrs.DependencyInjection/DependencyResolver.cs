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
using Cqrs.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection
{
    /// <summary>
    /// Provides an ability to resolve instances of objects using MS DI
    /// </summary>
    public class DependencyResolver : Configuration.DependencyResolver
	{
		/// <summary>
		/// The inner <see cref="IServiceCollection"/> used by this instance.
		/// </summary>
		public IServiceCollection Kernel { get; private set; }

		/// <summary>
		/// A collection of <see cref="Module"/> instances to load when we call <see cref="PrepareKernel"/>
		/// </summary>
		public static IList<Module> ModulesToLoad = new List<Module>();

		/// <summary>
		/// A user supplied <see cref="Func{TResult}"/> that will be called during <see cref="Start"/> to create and populate <see cref="Configuration.DependencyResolver.Current"/>.
		/// </summary>
		public static Func<IServiceCollection, DependencyResolver> DependencyResolverCreator { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="DependencyResolver"/>
		/// </summary>
		public DependencyResolver(IServiceCollection kernel)
		{
			Kernel = kernel;
			BindDependencyResolver(kernel);
		}

		/// <summary>
		/// Checks if <see cref="IDependencyResolver"/> has already been registered and if not, registers this instance to it.
		/// </summary>
		protected virtual void BindDependencyResolver(IServiceCollection services)
		{
			bool isDependencyResolverBound = services.Any(x => x.ServiceType == typeof(IDependencyResolver));
			if (!isDependencyResolverBound)
			{
				services.AddSingleton<IDependencyResolver>(this);
			}
		}

		/// <summary>
		/// Starts the <see cref="DependencyResolver"/>
		/// </summary>
		/// <remarks>
		/// this exists so the static constructor can be triggered.
		/// </remarks>
		public static void Start(IServiceCollection kernel, bool prepareProvidedKernel = false)
		{
			if (DependencyResolverCreator != null)
				Current = DependencyResolverCreator(kernel);
			else
				Current = new DependencyResolver(kernel);

			if (prepareProvidedKernel)
				PrepareKernel(kernel);
		}

		/// <summary>
		/// Calls <see cref="Module.Load(IServiceCollection)"/> on each <see cref="Module"/> in <see cref="ModulesToLoad"/>
		/// </summary>
		public static void PrepareKernel(IServiceCollection services)
		{
			foreach(var module in ModulesToLoad)
				module.Load(services);
		}

		/// <summary>
		/// Resolves a single instance for the specified <typeparamref name="T"/>.
		/// </summary>
		public override T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		/// <summary>
		/// Resolves a single instance for the specified <paramref name="type"/>.
		/// </summary>
		public override object Resolve(Type type)
		{
			return Kernel.Where(x => x.ServiceType == type).Single();
		}
	}
}