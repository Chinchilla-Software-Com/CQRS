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
	/// The <see cref="Exception"/> that is thrown when more than one <see cref="IHandler"/> if found for a given <see cref="IMessage"/> that only expects one.
	/// </summary>
	[Serializable]
	public abstract class MultipleHandlersRegisteredException : InvalidOperationException
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="MultipleHandlersRegisteredException"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of <see cref="IMessage"/> that expected only one <see cref="IHandler"/>.</param>
		protected MultipleHandlersRegisteredException(Type type)
			: base(string.Format("More than one handler is registered for type '{0}'. You cannot send to more than one handler.", type.FullName))
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="MultipleHandlersRegisteredException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		protected MultipleHandlersRegisteredException(string message)
			: base(message)
		{
		}
	}
}