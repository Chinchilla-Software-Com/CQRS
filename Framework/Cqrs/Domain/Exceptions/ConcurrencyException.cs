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
	[Serializable]
	public class ConcurrencyException : Exception
	{
		public ConcurrencyException(Guid id)
			: base(string.Format("A different version than expected was found in aggregate {0}", id))
		{
		}
	}
}