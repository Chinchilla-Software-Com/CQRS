#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Messages;

namespace Cqrs.Configuration
{
	/// <summary>
	/// Information about a <see cref="IHandler"/> delegate.
	/// </summary>
	public class HandlerDelegate : HandlerDelegate<dynamic>
	{
	}

	/// <summary>
	/// Information about a delegate.
	/// </summary>
	public class HandlerDelegate<T>
	{
		/// <summary>
		/// The delegate that gets executed.
		/// </summary>
		public Action<T> Delegate { get; set; }

		/// <summary>
		/// The <see cref="Type"/> of the targeted object that <see cref="Delegate"/> operates on.
		/// </summary>
		public Type TargetedType { get; set; }
	}
}