#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public interface ISagaRepository<TAuthenticationToken>
	{
		void Save<TSaga>(TSaga saga, int? expectedVersion = null)
			where TSaga : ISaga<TAuthenticationToken>;

		TSaga Get<TSaga>(Guid sagaId, IList<ISagaEvent<TAuthenticationToken>> events = null)
			where TSaga : ISaga<TAuthenticationToken>;
	}
}