#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Cqrs.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using EventData=EventStore.ClientAPI.EventData;

namespace Cqrs.EventStore
{
	/// <summary>
	/// A factory implementing <see cref="IEventDeserialiser{TAuthenticationToken}"/> and <see cref="IEventBuilder{TAuthenticationToken}"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class EventFactory<TAuthenticationToken> : IEventBuilder<TAuthenticationToken>, IEventDeserialiser<TAuthenticationToken>
	{
		#region Implementation of IEventDeserialiser

		/// <summary>
		/// Deserialise the provided <paramref name="eventData"/> into an <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="RecordedEvent"/> to Deserialise.</param>
		public IEvent<TAuthenticationToken> Deserialise(RecordedEvent eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			switch (eventData.EventType)
			{
				case "Client Connected":
					return JsonConvert.DeserializeObject<SimpleEvent<TAuthenticationToken>>(new UTF8Encoding().GetString(eventData.Data), jsonSerialiserSettings);
				default:
					return (IEvent<TAuthenticationToken>)JsonConvert.DeserializeObject(new UTF8Encoding().GetString(eventData.Data), Type.GetType(eventData.EventType));
			}
		}

		/// <summary>
		/// Deserialise the provided <paramref name="notification"/> into an <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		/// <param name="notification">The <see cref="ResolvedEvent"/> to Deserialise.</param>
		public IEvent<TAuthenticationToken> Deserialise(ResolvedEvent notification)
		{
			return Deserialise(notification.Event);
		}

		/// <summary>
		/// Gets the <see cref="JsonSerializerSettings"/> used while Deserialising.
		/// </summary>
		public JsonSerializerSettings GetSerialisationSettings()
		{
			return new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DateParseHandling = DateParseHandling.DateTimeOffset,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				Converters = new List<JsonConverter> { new StringEnumConverter() },
			};
		}

		#endregion

		#region Implementation of IEventBuilder

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="eventData"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to add to the <see cref="EventData"/>.</param>
		public EventData CreateFrameworkEvent(string type, IEvent<TAuthenticationToken> eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return new EventData
			(
				Guid.NewGuid(),
				type,
				true,
				new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(eventData, jsonSerialiserSettings)),
				new byte[0]
			);
		}

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="eventData"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to add to the <see cref="EventData"/>.</param>
		public EventData CreateFrameworkEvent(IEvent<TAuthenticationToken> eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return new EventData
			(
				Guid.NewGuid(),
				eventData.GetType().AssemblyQualifiedName,
				true,
				new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(eventData, jsonSerialiserSettings)),
				new byte[0]
			);
		}

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> from the provided <paramref name="eventDataBody"/>.
		/// </summary>
		/// <param name="eventDataBody">A JSON string of serialised data.</param>
		public EventData CreateFrameworkEvent(string eventDataBody)
		{
			return CreateFrameworkEvent
			(
				new SimpleEvent<TAuthenticationToken> { Id = Guid.NewGuid(), Message = eventDataBody, TimeStamp = DateTimeOffset.Now, Version = 1 }
			);
		}

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> from the provided <paramref name="eventDataBody"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="eventDataBody">A JSON string of serialised data.</param>
		public EventData CreateFrameworkEvent(string type, string eventDataBody)
		{
			return CreateFrameworkEvent
			(
				type,
				new SimpleEvent<TAuthenticationToken> { Id = Guid.NewGuid(), Message = eventDataBody, TimeStamp = DateTimeOffset.Now, Version = 1 }
			);
		}

		/// <summary>
		/// Create an <see cref="EventData"/> that notifies people a client has connected.
		/// </summary>
		/// <param name="clientName">The name of the client that has connected.</param>
		public EventData CreateClientConnectedEvent(string clientName)
		{
			return CreateFrameworkEvent
			(
				"Client Connected",
				string.Format("{0} has connected.", clientName)
			);
		}

		#endregion
	}
}