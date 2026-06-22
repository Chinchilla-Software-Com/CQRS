#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Commands;

namespace Cqrs.Domain.Exceptions
{
	/// <summary>
	/// The operation resulted in a duplicate.
	/// </summary>
	[Serializable]
	public class DuplicateCreateCommandException : Exception
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="DuplicateCreateCommandException"/> with a specified <paramref name="message">error message</paramref> and a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public DuplicateCreateCommandException(string message, Exception exception)
			: base(message, exception)
		{
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="DuplicateCreateCommandException"/> with a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public DuplicateCreateCommandException(Exception exception)
			: base("The operation resulted in a duplicate.", exception)
		{
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="DuplicateCreateCommandException"/> with 
		/// the <paramref name="commandType"/> and <paramref name="commandId"/> that was issued as well as
		/// a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="commandType">The <see cref="Type"/> of the <see cref="ICommand{TAuthenticationToken}"/> that was issued.</param>
		/// <param name="commandId">The <see cref="ICommand{TAuthenticationToken}.Id"/> of the <see cref="ICommand{TAuthenticationToken}"/> that was issued.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public DuplicateCreateCommandException(Type commandType, Guid commandId, Exception exception)
			: base(string.Format("The operation resulted in a duplicate for a command of type '{0}' with Rsn '{1}'", commandType.FullName, commandId), exception)
		{
			CommandType = commandType;
			CommandId = commandId;
		}

		/// <summary>
		/// The <see cref="Type"/> of the <see cref="ICommand{TAuthenticationToken}"/> that was issued.
		/// </summary>
		[DataMember]
		public Type CommandType { get; set; }

		/// <summary>
		/// The <see cref="ICommand{TAuthenticationToken}.Id"/> of the <see cref="ICommand{TAuthenticationToken}"/> that was issued.
		/// </summary>
		[DataMember]
		public Guid CommandId { get; set; }
	}
}