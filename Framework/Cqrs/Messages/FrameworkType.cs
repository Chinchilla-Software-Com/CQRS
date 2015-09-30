#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Messages
{
	public enum FrameworkType
	{
		Unknown = -2,

		External = -1,

		BuiltIn = 0,

		Akka = 1
	}
}