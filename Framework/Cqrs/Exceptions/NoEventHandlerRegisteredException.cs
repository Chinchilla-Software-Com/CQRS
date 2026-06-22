#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;

namespace Cqrs.Exceptions
{
	/// <summary>
	/// The <see cref="Exception"/> that is thrown when no <see cref="IEventHandler"/> if found for a given <see cref="IEvent{TAuthenticationToken}"/>.
	/// </summary>
	[Serializable]
	public class NoEventHandlerRegisteredException : NoHandlerRegisteredException
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="NoEventHandlerRegisteredException"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of <see cref="IEvent{TAuthenticationToken}"/> that expected an <see cref="IEventHandler"/>.</param>
		public NoEventHandlerRegisteredException(Type type)
			: base(string.Format("No event handler registered for type '{0}'", type.FullName))
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="NoEventHandlerRegisteredException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		protected NoEventHandlerRegisteredException(string message)
			: base(message)
		{
		}
	}
}