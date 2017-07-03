#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	public class RouteHandlerDelegate
	{
		public Action<IMessage> Delegate { get; set; }

		public Type TargetedType { get; set; }
	}
}