#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cqrs.Events
{
	/// <summary>
	/// Default settings for JSON serialisation  and deserialisation.
	/// </summary>
	public class DefaultJsonSerializerSettings
	{
		/// <summary>
		/// System wide default <see cref="JsonSerializerSettings"/>.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static DefaultJsonSerializerSettings()
		{
			DefaultSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DateParseHandling = DateParseHandling.DateTimeOffset,
				DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
				Converters = new List<JsonConverter> { new StringEnumConverter() },
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
				FloatFormatHandling = FloatFormatHandling.DefaultValue,
				NullValueHandling = NullValueHandling.Include,
				PreserveReferencesHandling = PreserveReferencesHandling.All,
				ReferenceLoopHandling = ReferenceLoopHandling.Error,
				StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
				TypeNameHandling = TypeNameHandling.All,
				ContractResolver = new EncryptedContractResolver()
			};
		}
	}
}