#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Commands;

namespace Cqrs.Exceptions
{
	/// <summary>
	/// The <see cref="Exception"/> that is thrown when more than one <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> if found for a given <see cref="ICommand{TAuthenticationToken}"/> that only expects one.
	/// </summary>
	[Serializable]
	public class MultipleCommandHandlersRegisteredException : MultipleHandlersRegisteredException
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="MultipleCommandHandlersRegisteredException"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of <see cref="ICommand{TAuthenticationToken}"/> that expected only one <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>.</param>
		public MultipleCommandHandlersRegisteredException(Type type)
			: base(string.Format("More than one command handler is registered for type '{0}'. You cannot send to a command more than one handler.", type.FullName))
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="MultipleCommandHandlersRegisteredException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public MultipleCommandHandlersRegisteredException(string message)
			: base(message)
		{
		}
	}
}