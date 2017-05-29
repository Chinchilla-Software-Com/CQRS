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
using Newtonsoft.Json.Serialization;

namespace Cqrs.Events
{
	public class DefaultEventBuilder<TAuthenticationToken> : EventBuilder<TAuthenticationToken>
	{
		public static IList<JsonConverter> DefaultJsonConverters { get; set; }

		public static IContractResolver DefaultJsonContractResolver { get; set; }

		static DefaultEventBuilder()
		{
			DefaultJsonConverters = new List<JsonConverter> {new StringEnumConverter()};
			DefaultJsonContractResolver = null;
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
			var jsonSerialiserSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DateParseHandling = DateParseHandling.DateTimeOffset,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc,
				Converters = new List<JsonConverter> { new StringEnumConverter() }
			};

			if (DefaultJsonConverters != null)
				jsonSerialiserSettings.Converters = DefaultJsonConverters;
			if (DefaultJsonContractResolver != null)
				jsonSerialiserSettings.ContractResolver = DefaultJsonContractResolver;

			return jsonSerialiserSettings;
		}
	}
}