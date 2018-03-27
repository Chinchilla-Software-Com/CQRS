#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Text;
using Cqrs.Snapshots;

namespace Cqrs.Events
{
	/// <summary>
	/// Builds <see cref="EventData"/> from various input formats.
	/// </summary>
	public abstract class SnapshotBuilder
		: ISnapshotBuilder
	{
		#region Implementation of IEventBuilder<TAuthenticationToken>

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="snapshot"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="snapshot">The <see cref="Snapshot"/> to add to the <see cref="EventData"/>.</param>
		public virtual EventData CreateFrameworkEvent(string type, Snapshot snapshot)
		{
			return new EventData
			{
				EventId = Guid.NewGuid(),
				EventType = type,
				Data = SerialiseEventDataToString(snapshot)
			};
		}

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="snapshot"/>.
		/// </summary>
		/// <param name="snapshot">The <see cref="Snapshot"/> to add to the <see cref="EventData"/>.</param>
		public virtual EventData CreateFrameworkEvent(Snapshot snapshot)
		{
			return CreateFrameworkEvent(snapshot.GetType().AssemblyQualifiedName, snapshot);
		}

		#endregion

		/// <summary>
		/// Serialise the provided <paramref name="snapshot"/> into a <see cref="byte"/> <see cref="Array"/>.
		/// </summary>
		/// <param name="snapshot">The <see cref="Snapshot"/> to serialise.</param>
		protected virtual byte[] SerialiseEventData(Snapshot snapshot)
		{
			return new UTF8Encoding().GetBytes(SerialiseEventDataToString(snapshot));
		}

		/// <summary>
		/// Serialise the provided <paramref name="snapshot"/> into a <see cref="string"/>.
		/// </summary>
		/// <param name="snapshot">The <see cref="Snapshot"/> to serialise.</param>
		protected abstract string SerialiseEventDataToString(Snapshot snapshot);
	}
}