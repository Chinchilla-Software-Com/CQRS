#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
			: base("The operation resulted in a duplicate ", exception)
		{
		}
	}
}