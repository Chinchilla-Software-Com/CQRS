#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Domain.Exceptions
{
	public class DuplicateCreateCommandException : Exception
	{
		public DuplicateCreateCommandException(string message, Exception exception)
			: base(message, exception)
		{
		}

		public DuplicateCreateCommandException(Exception exception)
			: base("The operation resulted in a duplicate.", exception)
		{
		}

		public DuplicateCreateCommandException(Type commandType, Guid commandRsn, Exception exception)
			: base(string.Format("The operation resulted in a duplicate for a command of type '{0}' with Rsn '{1}'", commandType.FullName, commandRsn), exception)
		{
		}
	}
}