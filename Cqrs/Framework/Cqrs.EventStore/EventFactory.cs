using System;
using System.Collections.Generic;
using System.Text;
using Cqrs.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cqrs.EventStore
{
	public class EventFactory<TPermissionToken> : IEventBuilder<TPermissionToken>, IEventDeserialiser<TPermissionToken>
	{
		#region Implementation of IEventDeserialiser

		public IEvent<TPermissionToken> Deserialise(RecordedEvent eventData)
		{
			var jsonSerialiserSettings = GetSerialisationSettings();

			switch (eventData.EventType)
			{
				case "Client Connected":
					return JsonConvert.DeserializeObject<SimpleEvent<TPermissionToken>>(new UTF8Encoding().GetString(eventData.Data), jsonSerialiserSettings);
				default:
					return (IEvent<TPermissionToken>)JsonConvert.DeserializeObject(new UTF8Encoding().GetString(eventData.Data), Type.GetType(eventData.EventType));
			}
		}

		public IEvent<TPermissionToken> Deserialise(ResolvedEvent notification)
		{
			return Deserialise(notification.Event);
		}

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

		public EventData CreateFrameworkEvent(string type, IEvent<TPermissionToken> eventData)
		{
			var jsonSerialiserSettings = GetSerialisationSettings();

			return new EventData
			(
				Guid.NewGuid(),
				type,
				true,
				new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(eventData, jsonSerialiserSettings)),
				new byte[0]
			);
		}

		public EventData CreateFrameworkEvent(IEvent<TPermissionToken> eventData)
		{
			var jsonSerialiserSettings = GetSerialisationSettings();

			return new EventData
			(
				Guid.NewGuid(),
				eventData.GetType().AssemblyQualifiedName,
				true,
				new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(eventData, jsonSerialiserSettings)),
				new byte[0]
			);
		}

		public EventData CreateFrameworkEvent(string eventDataBody)
		{
			return CreateFrameworkEvent
			(
				new SimpleEvent<TPermissionToken> { Id = Guid.NewGuid(), Message = eventDataBody, TimeStamp = DateTimeOffset.Now, Version = 1 }
			);
		}

		public EventData CreateFrameworkEvent(string type, string eventDataBody)
		{
			return CreateFrameworkEvent
			(
				type,
				new SimpleEvent<TPermissionToken> { Id = Guid.NewGuid(), Message = eventDataBody, TimeStamp = DateTimeOffset.Now, Version = 1 }
			);
		}

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