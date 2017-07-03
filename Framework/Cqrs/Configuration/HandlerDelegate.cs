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
	public class HandlerDelegate
	{
		public Action<dynamic> Delegate { get; set; }

		public Type TargetedType { get; set; }
	}
}