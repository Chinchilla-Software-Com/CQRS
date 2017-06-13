#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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