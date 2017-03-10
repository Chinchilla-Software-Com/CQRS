using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cqrs.Azure.BlobStorage
{
	public abstract class TableEntity<TData>
		: TableEntity
	{
		protected virtual TData Deserialise(string json)
		{
			using (var stringReader = new StringReader(json))
			using (var jsonTextReader = new JsonTextReader(stringReader))
				return GetSerialiser().Deserialize<TData>(jsonTextReader);
		}

		protected virtual string Serialise(TData data)
		{
			string dataContent = JsonConvert.SerializeObject(data, GetSerialisationSettings());

			return dataContent;
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

		protected virtual JsonSerializer GetSerialiser()
		{
			JsonSerializerSettings settings = GetSerialisationSettings();
			return JsonSerializer.Create(settings);
		}
	}
}