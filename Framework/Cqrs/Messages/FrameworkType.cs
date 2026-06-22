#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Messages
{
	/// <summary>
	/// Indicates a framework system or type.
	/// </summary>
	[Obsolete]
	public enum FrameworkType
	{
		/// <summary>
		/// An unknown framework.
		/// </summary>
		Unknown = -2,

		/// <summary>
		/// An external framework.
		/// </summary>
		External = -1,

		/// <summary>
		/// The built-in framework
		/// </summary>
		BuiltIn = 0,

		/// <summary>
		/// The CQRS Akka.NET framework
		/// </summary>
		Akka = 1
	}
}