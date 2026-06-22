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
	/// The <see cref="Exception"/> that is thrown when no <see cref="IHandler"/> if found for a given <see cref="IMessage"/>.
	/// </summary>
	[Serializable]
	public class NoHandlerRegisteredException : InvalidOperationException
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="NoHandlerRegisteredException"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of <see cref="IMessage"/> that expected a <see cref="IHandler"/>.</param>
		public NoHandlerRegisteredException(Type type)
			: base(string.Format("No handler is registered for type '{0}'.", type.FullName))
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="NoHandlerRegisteredException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public NoHandlerRegisteredException(string message)
			: base(message)
		{
		}
	}
}