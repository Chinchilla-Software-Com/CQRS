#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Cqrs.EventStore
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
		/// <param name="eventData">The <see cref="RecordedEvent"/> to Deserialise.</param>
		IEvent<TAuthenticationToken> Deserialise(RecordedEvent eventData);

		/// <summary>
		/// Deserialise the provided <paramref name="notification"/> into an <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		/// <param name="notification">The <see cref="ResolvedEvent"/> to Deserialise.</param>
		IEvent<TAuthenticationToken> Deserialise(ResolvedEvent notification);

#pragma warning disable CS0419 // Ambiguous reference in cref attribute
							  /// <summary>
							  /// Gets the <see cref="JsonSerializerSettings"/> used while Deserialising.
							  /// </summary>
		JsonSerializerSettings GetSerialisationSettings();
#pragma warning restore CS0419 // Ambiguous reference in cref attribute
	}
}