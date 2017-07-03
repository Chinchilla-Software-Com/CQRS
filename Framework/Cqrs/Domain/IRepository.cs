#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Domain
{
	[Obsolete("Use IAggregateRepository")]
	public interface IRepository<TAuthenticationToken>
		: IAggregateRepository<TAuthenticationToken> { }
}