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
using Azure;
using Azure.Data.Tables;
using Cqrs.Events;
using Newtonsoft.Json;

namespace Cqrs.Azure.Storage
{
	/// <summary>
	/// A projection/entity especially designed to work with Azure Table storage.
	/// </summary>
	[Serializable]
	[DataContract]
	public abstract class TableEntity<TData>
		: ITableEntity
	{
		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		/// <summary>
		/// The partition key is a unique identifier for the partition within a given table and forms the first part of an entity's primary key.
		/// </summary>
		/// <value>A string containing the partition key for the entity.</value>
		string ITableEntity.PartitionKey { get; set; }
		/// <summary>
		/// The row key is a unique identifier for an entity within a given partition. Together the <see cref="ITableEntity.PartitionKey" /> and RowKey uniquely identify every entity within a table.
		/// </summary>
		/// <value>A string containing the row key for the entity.</value>
		string ITableEntity.RowKey { get; set; }
		/// <summary>
		/// The Timestamp property is a DateTime value that is maintained on the server side to record the time an entity was last modified.
		/// The Table service uses the Timestamp property internally to provide optimistic concurrency. The value of Timestamp is a monotonically increasing value,
		/// meaning that each time the entity is modified, the value of Timestamp increases for that entity.
		/// This property should not be set on insert or update operations (the value will be ignored).
		/// </summary>
		/// <value>A <see cref="DateTimeOffset"/> containing the timestamp of the entity.</value>
		DateTimeOffset? ITableEntity.Timestamp { get; set; }
		/// <summary>
		/// Gets or sets the entity's ETag.
		/// </summary>
		/// <value>A string containing the ETag value for the entity.</value>
		ETag ITableEntity.ETag { get; set; }

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