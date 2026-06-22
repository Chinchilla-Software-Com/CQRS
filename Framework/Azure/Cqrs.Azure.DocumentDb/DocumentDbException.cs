#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Azure.DocumentDb
{
	/// <summary>
	/// A non-fault tolerant <see cref="Exception"/> while interacting with DocumentDB.
	/// </summary>
	[Serializable]
	public class DocumentDbException : Exception
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="DocumentDbException"/> with a specified error message and a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public DocumentDbException(string message, Exception exception)
			: base(message, exception)
		{
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="DocumentDbException"/> with a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public DocumentDbException(Exception exception)
			: this("Non-fault tolerant exception raised.", exception)
		{
		}
	}
}