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
	public class DuplicateEventException<TAggregateRoot, TAuthenticationToken> : Exception
		where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		public DuplicateEventException(Guid id, int version)
			: base(string.Format("Eventstore gave more than one event for aggregate '{0}' of type '{1}' for version {2}", id, typeof(TAggregateRoot).FullName, version))
		{
		}
	}
}