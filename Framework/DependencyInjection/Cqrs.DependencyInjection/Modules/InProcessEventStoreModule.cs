#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Modules
{
	/// <summary>
	/// A <see cref="Module"/> that configures the <see cref="InProcessEventStore{TAuthenticationToken}"/> as a <see cref="IEventStore{TAuthenticationToken}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class InProcessEventStoreModule<TAuthenticationToken> : ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load(IServiceCollection services)
		{
			RegisterFactories(services);
			RegisterServices(services);
			RegisterEventStore(services);
		}

		#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories(IServiceCollection services)
		{
		}

		/// <summary>
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices(IServiceCollection services)
		{
		}

		/// <summary>
		/// Register the <see cref="InProcessEventStore{TAuthenticationToken}"/>
		/// </summary>
		public virtual void RegisterEventStore(IServiceCollection services)
		{
			services.AddSingleton<IEventStore<TAuthenticationToken>, InProcessEventStore<TAuthenticationToken>>();
		}
	}
}
