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
	/// Builds <see cref="EventData"/> from various input formats serialising as JSON.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class DefaultEventBuilder<TAuthenticationToken> : EventBuilder<TAuthenticationToken>
	{
		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static DefaultEventBuilder()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		#region Implementation of EventBuilder

		/// <summary>
		/// Serialise the provided <paramref name="eventData"/> into JSON a <see cref="string"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to serialise.</param>
		protected override string SerialiseEventDataToString(IEvent<TAuthenticationToken> eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return JsonConvert.SerializeObject(eventData, jsonSerialiserSettings);
		}

		#endregion

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