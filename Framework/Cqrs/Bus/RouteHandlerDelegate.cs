#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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