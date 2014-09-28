#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Cqrs.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cqrs.Azure.DocumentDb
{
	public class AzureDocumentDbEventDeserialiser<TAuthenticationToken> : IEventDeserialiser<TAuthenticationToken>
	{
		public IEvent<TAuthenticationToken> Deserialise(EventData eventData)
		{
			return (IEvent<TAuthenticationToken>)JsonConvert.DeserializeObject(new UTF8Encoding().GetString(eventData.Data), Type.GetType(eventData.EventType));
		}

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