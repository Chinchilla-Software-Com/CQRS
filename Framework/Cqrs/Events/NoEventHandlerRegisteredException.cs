#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Bus;

namespace Cqrs.Events
{
	public class NoEventHandlerRegisteredException : NoHandlerRegisteredException
	{
		public NoEventHandlerRegisteredException(Type type)
			: base(string.Format("No event handler registered for type '{0}'", type.FullName))
		{
		}

		protected NoEventHandlerRegisteredException(string message)
			: base(message)
		{
		}
	}
}