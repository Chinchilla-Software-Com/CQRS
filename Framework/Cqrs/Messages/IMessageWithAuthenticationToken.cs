#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Messages
{
	/// <summary>
	/// A <see cref="IMessage"/> with authentication information.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface IMessageWithAuthenticationToken<TAuthenticationToken> : IMessage
	{
		/// <summary>
		/// The <typeparamref name="TAuthenticationToken"/> of the entity that triggered the event to be raised.
		/// </summary>
		TAuthenticationToken AuthenticationToken { get; set; }
	}
}