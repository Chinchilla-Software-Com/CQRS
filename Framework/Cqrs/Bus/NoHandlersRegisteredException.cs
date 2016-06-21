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
	public abstract class NoHandlersRegisteredException : InvalidOperationException
	{
		protected NoHandlersRegisteredException(Type type)
			: base(string.Format("No handlers are registered for type '{0}'.", type.FullName))
		{
		}

		protected NoHandlersRegisteredException(string message)
			: base(message)
		{
		}
	}
}