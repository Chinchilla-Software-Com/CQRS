#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Events
{
	/// <summary>
	/// Deserialises <see cref="IEvent{TAuthenticationToken}"/> from a serialised state.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface IEventDeserialiser<TAuthenticationToken>
	{
		/// <summary>
		/// Deserialise the provided <paramref name="eventData"/> into an <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to Deserialise.</param>
		IEvent<TAuthenticationToken> Deserialise(EventData eventData);
	}
}