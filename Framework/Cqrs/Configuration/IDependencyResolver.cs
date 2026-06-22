#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Configuration
{
	/// <summary>
	/// Provides an ability to resolve instances of objects.
	/// </summary>
	public interface IDependencyResolver 
	{
		/// <summary>
		/// Resolves a single instance for the specified <typeparamref name="T"/>.
		/// Different implementations may return the first or last instance found or may return an exception.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object you want to resolve.</typeparam>
		/// <returns>An instance of type <typeparamref name="T"/>.</returns>
		T Resolve<T>();

		/// <summary>
		/// Resolves a single instance for the specified <paramref name="type"/>.
		/// Different implementations may return the first or last instance found or may return an exception.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of object you want to resolve.</param>
		/// <returns>An instance of type <paramref name="type"/>.</returns>
		object Resolve(Type type);
	}
}