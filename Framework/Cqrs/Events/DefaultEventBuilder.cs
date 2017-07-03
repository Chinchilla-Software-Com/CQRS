#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Newtonsoft.Json;

namespace Cqrs.Events
{
	public class DefaultEventBuilder<TAuthenticationToken> : EventBuilder<TAuthenticationToken>
	{
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static DefaultEventBuilder()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		#region Implementation of EventBuilder

		protected override string SerialiseEventDataToString(IEvent<TAuthenticationToken> eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return JsonConvert.SerializeObject(eventData, jsonSerialiserSettings);
		}

		#endregion

		protected virtual JsonSerializerSettings GetSerialisationSettings()
		{
			return DefaultSettings;
		}
	}
}