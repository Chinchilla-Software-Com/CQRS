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
	public class DocumentDbException : Exception
	{
		public DocumentDbException(string message, Exception exception)
			: base(message, exception)
		{
		}
	}
}