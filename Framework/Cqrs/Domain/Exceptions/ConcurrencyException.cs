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
	[Serializable]
	public class ConcurrencyException : Exception
	{
		public ConcurrencyException(Guid id)
			: base(string.Format("A different version than expected was found in aggregate {0}", id))
		{
		}
	}
}