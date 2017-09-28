#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Domain;

namespace Cqrs.Events
{
	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that can identify an <see cref="IAggregateRoot{TAuthenticationToken}">aggregate</see>.
	/// The <see cref="IEvent{TAuthenticationToken}.Id"/> is specifically the identifier for the individual <see cref="IEvent{TAuthenticationToken}"/> itself, not the <see cref="IAggregateRoot{TAuthenticationToken}">aggregate</see> being targeted.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface IEventWithIdentity<TAuthenticationToken> : IEvent<TAuthenticationToken>
	{
		/// <summary>
		/// The identity of the <see cref="IAggregateRoot{TAuthenticationToken}">aggregate</see> being targeted.
		/// </summary>
		[DataMember]
		Guid Rsn { get; set; }
	}
}