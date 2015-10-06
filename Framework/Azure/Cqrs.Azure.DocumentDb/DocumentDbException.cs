#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Azure.DocumentDb
{
	public class DocumentDbException : Exception
	{
		public DocumentDbException(string message, Exception exception)
			: base(message, exception)
		{
		}
	}
}