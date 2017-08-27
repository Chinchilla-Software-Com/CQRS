#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Runtime.Serialization;
using Cqrs.Events;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Cqrs.Azure.BlobStorage
{
	/// <summary>
	/// A projection/entity especially designed to work with Azure Table storage.
	/// </summary>
	[Serializable]
	[DataContract]
	public abstract class TableEntity<TData>
		: TableEntity
	{
		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static TableEntity()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		/// <summary>
		/// Deserialise the provided <paramref name="json"/> from its <see cref="string"/> representation.
		/// </summary>
		/// <param name="json">A <see cref="string"/> representation of an <typeparamref name="TData"/> to deserialise.</param>
		protected virtual TData Deserialise(string json)
		{
			using (var stringReader = new StringReader(json))
			using (var jsonTextReader = new JsonTextReader(stringReader))
				return GetSerialiser().Deserialize<TData>(jsonTextReader);
		}

		/// <summary>
		/// Serialise the provided <paramref name="data"/>.
		/// </summary>
		/// <param name="data">The <typeparamref name="TData"/> being serialised.</param>
		/// <returns>A <see cref="string"/> representation of the provided <paramref name="data"/>.</returns>
		protected virtual string Serialise(TData data)
		{
			string dataContent = JsonConvert.SerializeObject(data, GetSerialisationSettings());

			return dataContent;
		}

		/// <summary>
		/// Returns <see cref="DefaultSettings"/>
		/// </summary>
		/// <returns><see cref="DefaultSettings"/></returns>
		protected virtual JsonSerializerSettings GetSerialisationSettings()
		{
			return DefaultSettings;
		}

		/// <summary>
		/// Creates a new <see cref="JsonSerializer"/> using the settings from <see cref="GetSerialisationSettings"/>.
		/// </summary>
		/// <returns>A new instance of <see cref="JsonSerializer"/>.</returns>
		protected virtual JsonSerializer GetSerialiser()
		{
			JsonSerializerSettings settings = GetSerialisationSettings();
			return JsonSerializer.Create(settings);
		}
	}
}