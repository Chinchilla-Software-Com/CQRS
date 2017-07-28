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
	public class HandlerDelegate
	{
		/// <summary>
		/// The delegate that gets executed.
		/// </summary>
		public Action<dynamic> Delegate { get; set; }

		/// <summary>
		/// The <see cref="Type"/> of the targeted object that <see cref="Delegate"/> operates on.
		/// </summary>
		public Type TargetedType { get; set; }
	}
}