#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;
using EventData = EventStore.ClientAPI.EventData;

namespace Cqrs.EventStore
{
	/// <summary>
	/// Builds <see cref="EventData"/> from various input formats.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface IEventBuilder<TAuthenticationToken>
	{
		/// <summary>
		/// Create an <see cref="EventData">framework event</see> from the provided <paramref name="eventDataBody"/>.
		/// </summary>
		/// <param name="eventDataBody">A JSON string of serialised data.</param>
		EventData CreateFrameworkEvent(string eventDataBody);

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="eventData"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to add to the <see cref="EventData"/>.</param>
		EventData CreateFrameworkEvent(IEvent<TAuthenticationToken> eventData);

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="eventData"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to add to the <see cref="EventData"/>.</param>
		EventData CreateFrameworkEvent(string type, IEvent<TAuthenticationToken> eventData);

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> from the provided <paramref name="eventDataBody"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="eventDataBody">A JSON string of serialised data.</param>
		EventData CreateFrameworkEvent(string type, string eventDataBody);

		/// <summary>
		/// Create an <see cref="EventData"/> that notifies people a client has connected.
		/// </summary>
		/// <param name="clientName">The name of the client that has connected.</param>
		EventData CreateClientConnectedEvent(string clientName);
	}
}