#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Snapshots;
using Newtonsoft.Json;

namespace Cqrs.Events
{
	/// <summary>
	/// Deserialises <see cref="Snapshot"/> from a serialised state.
	/// </summary>
	public class SnapshotDeserialiser
		: ISnapshotDeserialiser
	{
		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static SnapshotDeserialiser()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		/// <summary>
		/// Deserialise the provided <paramref name="eventData"/> into an <see cref="Snapshot"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to Deserialise.</param>
		public virtual Snapshot Deserialise(EventData eventData)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return (Snapshot)JsonConvert.DeserializeObject((string)eventData.Data, Type.GetType(eventData.EventType), jsonSerialiserSettings);
		}

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