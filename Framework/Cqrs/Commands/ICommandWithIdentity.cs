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

namespace Cqrs.Commands
{
	/// <summary>
	/// An <see cref="ICommand{TAuthenticationToken}"/> that can identify an <see cref="IAggregateRoot{TAuthenticationToken}">aggregate</see>.
	/// The <see cref="ICommand{TAuthenticationToken}.Id"/> is specifically the identifier for the individual <see cref="ICommand{TAuthenticationToken}"/> itself, not the <see cref="IAggregateRoot{TAuthenticationToken}">aggregate</see> being targeted.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface ICommandWithIdentity<TAuthenticationToken> : ICommand<TAuthenticationToken>
	{
		/// <summary>
		/// The identity of the <see cref="IAggregateRoot{TAuthenticationToken}">aggregate</see> being targeted.
		/// </summary>
		[DataMember]
		Guid Rsn { get; set; }
	}
}