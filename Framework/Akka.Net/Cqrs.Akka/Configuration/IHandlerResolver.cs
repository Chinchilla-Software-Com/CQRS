#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Akka.Configuration
{
	/// <summary>
	/// Resolves handlers for use with Akka.Net
	/// </summary>
	public interface IHandlerResolver
	{
		/// <summary>
		/// Resolves instances of <paramref name="handerType"/>.
		/// </summary>
		object Resolve(Type handerType, object rsn);
	}
}