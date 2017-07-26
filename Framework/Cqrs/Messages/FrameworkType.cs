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
		Unknown = -2,

		External = -1,

		BuiltIn = 0,

		Akka = 1
	}
}