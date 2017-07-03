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
	public abstract class MultipleHandlersRegisteredException : InvalidOperationException
	{
		protected MultipleHandlersRegisteredException(Type type)
			: base(string.Format("More than one handler is registered for type '{0}'. You cannot send to more than one handler.", type.FullName))
		{
		}

		protected MultipleHandlersRegisteredException(string message)
			: base(message)
		{
		}
	}
}