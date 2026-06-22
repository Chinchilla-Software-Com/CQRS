#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Snapshots;
using Newtonsoft.Json;

namespace Cqrs.Events
{
	/// <summary>
	/// Builds <see cref="EventData"/> from various input formats serialising as JSON.
	/// </summary>
	public class DefaultSnapshotBuilder
		: SnapshotBuilder
	{
		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static DefaultSnapshotBuilder()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		#region Implementation of SnapshotBuilder

		/// <summary>
		/// Serialise the provided <paramref name="snapshot"/> into JSON a <see cref="string"/>.
		/// </summary>
		/// <param name="snapshot">The <see cref="Snapshot"/> to serialise.</param>
		protected override string SerialiseEventDataToString(Snapshot snapshot)
		{
			JsonSerializerSettings jsonSerialiserSettings = GetSerialisationSettings();

			return JsonConvert.SerializeObject(snapshot, jsonSerialiserSettings);
		}

		#endregion

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