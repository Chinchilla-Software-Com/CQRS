#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Newtonsoft.Json;

namespace Cqrs.Events
{
	public class EventDeserialiser<TAuthenticationToken> : IEventDeserialiser<TAuthenticationToken>
	{
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static EventDeserialiser()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		public virtual IEvent<TAuthenticationToken> Deserialise(EventData eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return (IEvent<TAuthenticationToken>)JsonConvert.DeserializeObject((string)eventData.Data, Type.GetType(eventData.EventType), jsonSerialiserSettings);
		}

		protected virtual JsonSerializerSettings GetSerialisationSettings()
		{
			return DefaultSettings;
		}
	}
}