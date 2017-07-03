#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Bus
{
	public class NoHandlerRegisteredException : InvalidOperationException
	{
		public NoHandlerRegisteredException(Type type)
			: base(string.Format("No handler is registered for type '{0}'.", type.FullName))
		{
		}

		public NoHandlerRegisteredException(string message)
			: base(message)
		{
		}
	}
}