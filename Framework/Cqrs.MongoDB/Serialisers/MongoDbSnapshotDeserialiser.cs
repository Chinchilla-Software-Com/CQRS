#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Events;
using Cqrs.Snapshots;

namespace Cqrs.MongoDB.Serialisers
{
	/// <summary>
	/// Deserialises <see cref="Snapshot"/> from a serialised state.
	/// </summary>
	public class MongoDbSnapshotDeserialiser : SnapshotDeserialiser
	{
		#region Overrides of EventDeserialiser<TAuthenticationToken>

		/// <summary>
		/// Deserialise the provided <paramref name="eventData"/> into an <see cref="Snapshot"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to Deserialise.</param>
		public override Snapshot Deserialise(EventData eventData)
		{
			return (Snapshot)eventData.Data;
		}

		#endregion
	}
}