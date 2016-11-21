#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Messages
{
	[Obsolete]
	public enum FrameworkType
	{
		Unknown = -2,

		External = -1,

		BuiltIn = 0,

		Akka = 1
	}
}