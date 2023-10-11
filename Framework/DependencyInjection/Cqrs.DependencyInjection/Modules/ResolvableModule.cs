#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Cqrs.DependencyInjection.Modules
{
	/// <summary>
	/// A <see cref="Module"/> that can resolve anything bound before being called.
	/// </summary>
	public abstract class ResolvableModule : Module
	{
		/// <summary>
		/// Resolves instances for the specified <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to resolve.</typeparam>
		/// <param name="services">The <see cref="IServiceCollection"/> to check.</param>
		/// <returns>Null if no resolution is made.</returns>
		protected virtual T Resolve<T>(IServiceCollection services)
		{
			return (T)Resolve(services, typeof(T));
		}

		/// <summary>
		/// Resolves instances for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to check.</param>
		/// <param name="type">The <see cref="Type"/> to resolve.</param>
		/// <returns>Null if no resolution is made.</returns>
		protected virtual object Resolve(IServiceCollection services, Type type)
		{
			using (ServiceProvider tempProvider = services.BuildServiceProvider())
			{
				// temporarily set the Kernel
				var di = DependencyResolver.Current as DependencyResolver;
				bool setTemporaryKernel = di != null && di.Kernel == null;
				if (setTemporaryKernel)
					di.SetKernel(tempProvider);

				object result = tempProvider.GetService(type);

				// resolve the Kernel back to null
				if (setTemporaryKernel)
					di.SetKernel(null);

				return result;
			}

		}
	}
}