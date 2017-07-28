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
	/// <summary>
	/// Deserialises <see cref="IEvent{TAuthenticationToken}"/> from a serialised state.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class EventDeserialiser<TAuthenticationToken> : IEventDeserialiser<TAuthenticationToken>
	{
		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static EventDeserialiser()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		/// <summary>
		/// Deserialise the provided <paramref name="eventData"/> into an <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to Deserialise.</param>
		public virtual IEvent<TAuthenticationToken> Deserialise(EventData eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return (IEvent<TAuthenticationToken>)JsonConvert.DeserializeObject((string)eventData.Data, Type.GetType(eventData.EventType), jsonSerialiserSettings);
		}

		/// <summary>
		/// Returns <see cref="DefaultSettings"/>
		/// </summary>
		/// <returns><see cref="DefaultSettings"/></returns>
		protected virtual JsonSerializerSettings GetSerialisationSettings()
		{
			return DefaultSettings;
		}
	}
}