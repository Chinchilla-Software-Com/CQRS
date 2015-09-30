#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Bus
{
	public class MultipleCommandHandlersRegisteredException : MultipleHandlersRegisteredException
	{
		public MultipleCommandHandlersRegisteredException(Type type)
			: base(string.Format("More than one command handler is registered for type '{0}'. You cannot send to a command more than one handler.", type.FullName))
		{
		}

		public MultipleCommandHandlersRegisteredException(string message)
			: base(message)
		{
		}
	}
}