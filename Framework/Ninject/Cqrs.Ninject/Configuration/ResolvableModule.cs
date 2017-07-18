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
		protected virtual T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		protected virtual object Resolve(Type serviceType)
		{
			return Kernel.Resolve(Kernel.CreateRequest(serviceType, null, new Parameter[0], true, true)).SingleOrDefault();
		}
	}
}