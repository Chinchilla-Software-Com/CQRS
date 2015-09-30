#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Bus;

namespace Cqrs.Commands
{
	public class NoCommandHandlerRegisteredException : NoHandlerRegisteredException
	{
		public NoCommandHandlerRegisteredException(Type type)
			: base(string.Format("No command handler is registered for type '{0}'.", type.FullName))
		{
		}

		protected NoCommandHandlerRegisteredException(string message)
			: base(message)
		{
		}
	}
}