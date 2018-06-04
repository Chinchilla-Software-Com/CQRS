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
	/// The <see cref="Exception"/> that is thrown when a given <see cref="IMessage"/> fails authorisation.
	/// </summary>
	[Serializable]
	public class UnAuthorisedMessageReceivedException : UnauthorizedAccessException
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="UnAuthorisedMessageReceivedException"/>.
		/// </summary>
		public UnAuthorisedMessageReceivedException(string typeName, string id, object identifyMessage)
			: base(string.Format("An event message arrived with the {0} was of type {1}{2} and was not authorized.", id, typeName, identifyMessage))
		{
		}

		/// <summary>
		/// Instantiates a new instance of <see cref="UnAuthorisedMessageReceivedException"/> with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public UnAuthorisedMessageReceivedException(string message)
			: base(message)
		{
		}
	}
}