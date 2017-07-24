#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Ninject.Modules;
using Ninject.Parameters;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that can resolve anything bound before being called.
	/// </summary>
	public abstract class ResolvableModule : NinjectModule
	{
		/// <summary>
		/// Resolves instances for the specified <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to resolve.</typeparam>
		/// <returns>Null if no resolution is made.</returns>
		protected virtual T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		/// <summary>
		/// Resolves instances for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to resolve.</param>
		/// <returns>Null if no resolution is made.</returns>
		protected virtual object Resolve(Type type)
		{
			return Kernel.Resolve(Kernel.CreateRequest(type, null, new Parameter[0], true, true)).SingleOrDefault();
		}
	}
}