#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that configures the <see cref="InProcessEventStore{TAuthenticationToken}"/> as a <see cref="IEventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class InProcessEventStoreModule<TAuthenticationToken> : ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterFactories();
			RegisterServices();
			RegisterEventStore();
		}

		#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
		}

		/// <summary>
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices()
		{
		}

		/// <summary>
		/// Register the <see cref="InProcessEventStore{TAuthenticationToken}"/>
		/// </summary>
		public virtual void RegisterEventStore()
		{
			Bind<IEventStore<TAuthenticationToken>>()
				.To<InProcessEventStore<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}
