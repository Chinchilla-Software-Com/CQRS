#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Exceptions
{
	/// <summary>
	/// A configuration caused an <see cref="Exception"/>.
	/// </summary>
	[Serializable]
	public class InvalidConfigurationException : Exception
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="InvalidConfigurationException"/> with a specified error message and a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public InvalidConfigurationException(string message, Exception exception)
			: base (message, exception)
		{
		}
	}
}