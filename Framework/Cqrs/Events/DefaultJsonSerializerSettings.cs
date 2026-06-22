#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Cqrs.Azure.KeyVault;
using Cqrs.Configuration;
using Cqrs.Messages;
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
				ContractResolver = new EncryptedContractResolver(new ConfigurationSecretFactory(DependencyResolver.ConfigurationManager ?? new ConfigurationManager()))
			};
		}

		/// <summary>
		/// Reconfigures the ContractResolver on the <see cref="DefaultSettings"/> to use the provided <paramref name="encryptedContractResolver"/>
		/// </summary>
		/// <param name="encryptedContractResolver">The <see cref="EncryptedContractResolver"/> to use</param>
		public static void ReconfigureEncryptedContractResolver(EncryptedContractResolver encryptedContractResolver)
		{
			DefaultSettings.ContractResolver = encryptedContractResolver;
		}

		/// <summary>
		/// Reconfigures the ContractResolver on the <see cref="DefaultSettings"/> to use the provided <paramref name="secretFactory"/> within the <see cref="EncryptedContractResolver"/>
		/// </summary>
		/// <param name="secretFactory">The <see cref="ISecretFactory"/> to use in the <see cref="EncryptedContractResolver"/>.</param>
		public static void ReconfigureEncryptedContractResolver(ISecretFactory secretFactory)
		{
			ReconfigureEncryptedContractResolver(new EncryptedContractResolver(secretFactory));
		}
	}
}