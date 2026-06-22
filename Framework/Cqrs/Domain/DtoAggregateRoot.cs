#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;

namespace Cqrs.Domain
{
	/// <summary>
	/// An <see cref="IAggregateRoot{TAuthenticationToken}"/> for operating on <see cref="IDto"/> instances.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	/// <typeparam name="TDto">The <see cref="Type"/> of <see cref="IDto"/>.</typeparam>
	public class DtoAggregateRoot<TAuthenticationToken, TDto> : AggregateRoot<TAuthenticationToken>
		where TDto : IDto
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="DtoAggregateRoot{TAuthenticationToken,TDto}"/>
		/// and instantly apply the change as n <see cref="DtoAggregateEvent{TAuthenticationToken,TDto}"/>
		/// </summary>
		/// <param name="id">The identifier of the <see cref="IDto"/>.</param>
		/// <param name="original">The original copy of the <see cref="IDto"/>. May be null for a <see cref="DtoAggregateEventType.Created"/> operation.</param>
		/// <param name="new">The new copy of the <see cref="IDto"/>. May be null for a <see cref="DtoAggregateEventType.Deleted"/> operation.</param>
		public DtoAggregateRoot(Guid id, TDto original, TDto @new)
		{
			Id = id;
			ApplyChange(new DtoAggregateEvent<TAuthenticationToken, TDto>(id, original, @new));
		}
	}
}