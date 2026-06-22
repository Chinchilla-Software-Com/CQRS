#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cqrs.DependencyInjection.Modules
{
	/// <summary>
	/// A plugable unit that defines resolution rules.
	/// </summary>
	public abstract class Module
	{
		/// <summary>
		/// Gets the module's name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Loads the definition.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to check.</param>
		public abstract void Load(IServiceCollection services);

		/// <summary>
		/// Indicates if the provided <typeparamref name="T"/> is already registered or not.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to check.</typeparam>
		/// <param name="services">The <see cref="IServiceCollection"/> to check.</param>
		public virtual bool IsRegistered<T>(IServiceCollection services)
		{
			return IsRegistered(services, typeof(T));
		}

		/// <summary>
		/// Indicates if the provided <paramref name="type"/> is already registered or not.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to check.</param>
		/// <param name="type">The <see cref="Type"/> to check.</param>
		public virtual bool IsRegistered(IServiceCollection services, Type type)
		{
			return services.Any(x => x.ServiceType == type);
		}

		/// <summary>
		/// Unregisters all bindings for the specified <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to remove.</typeparam>
		/// <param name="services">The <see cref="IServiceCollection"/> to check.</param>
		public virtual void Unbind<T>(IServiceCollection services)
		{
			Unbind(services, typeof(T));
		}

		/// <summary>
		/// Unregisters all bindings for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to check.</param>
		/// <param name="type">The <see cref="Type"/> to remove.</param>
		public virtual void Unbind(IServiceCollection services, Type type)
		{
			IEnumerable<ServiceDescriptor> bindings = services.Where(x => x.ServiceType == type).ToList();

			foreach (ServiceDescriptor binding in bindings)
				services.Remove(binding);
		}
	}
}