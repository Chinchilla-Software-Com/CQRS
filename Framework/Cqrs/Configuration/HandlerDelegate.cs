#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
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
		public
#if NET40
			Action<T>
#else
			Func<T, Task>
#endif
				Delegate { get; set; }

		/// <summary>
		/// The <see cref="Type"/> of the targeted object that <see cref="Delegate"/> operates on.
		/// </summary>
		public Type TargetedType { get; set; }
	}
}