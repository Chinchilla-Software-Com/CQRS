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
	/// The <see cref="Exception"/> that is thrown when no <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> if found for a given <see cref="ICommand{TAuthenticationToken}"/>.
	/// </summary>
	[Serializable]
	public class NoCommandHandlerRegisteredException : NoHandlerRegisteredException
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="NoCommandHandlerRegisteredException"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of <see cref="ICommand{TAuthenticationToken}"/> that expected an <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>.</param>
		public NoCommandHandlerRegisteredException(Type type)
			: base(string.Format("No command handler is registered for type '{0}'.", type.FullName))
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="NoCommandHandlerRegisteredException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		protected NoCommandHandlerRegisteredException(string message)
			: base(message)
		{
		}
	}
}