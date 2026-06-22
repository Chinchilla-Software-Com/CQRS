#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Snapshots;

namespace Cqrs.Events
{
	/// <summary>
	/// Builds <see cref="EventData"/> from various input formats.
	/// </summary>
	public interface ISnapshotBuilder
	{
		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="snapshot"/>.
		/// </summary>
		/// <param name="snapshot">The <see cref="Snapshot"/> to add to the <see cref="EventData"/>.</param>
		EventData CreateFrameworkEvent(Snapshot snapshot);

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="snapshot"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="snapshot">The <see cref="Snapshot"/> to add to the <see cref="EventData"/>.</param>
		EventData CreateFrameworkEvent(string type, Snapshot snapshot);
	}
}