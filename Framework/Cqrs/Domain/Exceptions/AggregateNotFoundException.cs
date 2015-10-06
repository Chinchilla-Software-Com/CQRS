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
	public class AggregateNotFoundException<TAggregateRoot, TAuthenticationToken> : Exception
		where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		public AggregateNotFoundException(Guid id)
			: base(string.Format("Aggregate '{0}' of type '{1}' was not found", id, typeof(TAggregateRoot).FullName))
		{
		}
	}
}