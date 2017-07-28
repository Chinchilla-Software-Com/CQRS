#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Messages;

namespace Cqrs.Exceptions
{
	/// <summary>
	/// The <see cref="Exception"/> that is thrown when no <see cref="IHandler"/> if found for a given <see cref="IMessage"/>, but more than one was expected.
	/// </summary>
	[Serializable]
	public abstract class NoHandlersRegisteredException : InvalidOperationException
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="NoHandlersRegisteredException"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of <see cref="IMessage"/> that expected more than one <see cref="IHandler"/>.</param>
		protected NoHandlersRegisteredException(Type type)
			: base(string.Format("No handlers are registered for type '{0}'.", type.FullName))
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="NoHandlersRegisteredException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		protected NoHandlersRegisteredException(string message)
			: base(message)
		{
		}
	}
}