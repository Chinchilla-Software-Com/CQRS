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
	public abstract class NoHandlerRegisteredException : InvalidOperationException
	{
		protected NoHandlerRegisteredException(Type type)
			: base(string.Format("No handler is registered for type '{0}'.", type.FullName))
		{
		}

		protected NoHandlerRegisteredException(string message)
			: base(message)
		{
		}
	}
}