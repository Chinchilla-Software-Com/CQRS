#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cqrs.Events
{
	public class DefaultEventBuilder<TAuthenticationToken> : EventBuilder<TAuthenticationToken>
	{
		#region Implementation of EventBuilder

		protected override string SerialiseEventDataToString(IEvent<TAuthenticationToken> eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return JsonConvert.SerializeObject(eventData, jsonSerialiserSettings);
		}

		#endregion

		protected virtual JsonSerializerSettings GetSerialisationSettings()
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
	}
}