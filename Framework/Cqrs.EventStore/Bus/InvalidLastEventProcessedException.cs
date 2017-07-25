#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using EventStore.ClientAPI;

namespace Cqrs.EventStore.Bus
{
	/// <summary>
	/// The <see cref="Position"/> information provided was invalid and a <see cref="Position"/> was unable to be created.
	/// </summary>
	[Serializable]
	public class InvalidLastEventProcessedException : Exception
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="InvalidLastEventProcessedException"/>.
		/// </summary>
		public InvalidLastEventProcessedException()
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="InvalidLastEventProcessedException"/> with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public InvalidLastEventProcessedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="InvalidLastEventProcessedException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public InvalidLastEventProcessedException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="InvalidLastEventProcessedException"/> with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected InvalidLastEventProcessedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}