#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Configuration
{
	public class HandlerDelegate
	{
		public Action<dynamic> Delegate { get; set; }

		public Type TargetedType { get; set; }
	}
}