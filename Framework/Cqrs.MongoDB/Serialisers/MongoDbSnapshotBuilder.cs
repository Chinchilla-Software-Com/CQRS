#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;
using Cqrs.Snapshots;

namespace Cqrs.MongoDB.Serialisers
{
	/// <summary>
	/// Builds <see cref="EventData"/> from various input formats.
	/// </summary>
	public class MongoDbSnapshotBuilder
		: SnapshotBuilder
	{
		#region Implementation of IEventBuilder<TAuthenticationToken>

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="snapshot"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="snapshot">The <see cref="Snapshot"/> to add to the <see cref="EventData"/>.</param>
		public override EventData CreateFrameworkEvent(string type, Snapshot snapshot)
		{
			return new EventData
			{
				EventId = Guid.NewGuid(),
				EventType = type,
				Data = snapshot
			};
		}

		/// <summary>
		/// Serialise the provided <paramref name="snapshot"/> into a <see cref="string"/>.
		/// </summary>
		/// <param name="snapshot">The <see cref="Snapshot"/> to serialise.</param>
		protected override string SerialiseEventDataToString(Snapshot snapshot)
		{
			throw new InvalidOperationException("MongoDB doesn't use strings.");
		}

		#endregion
	}
}