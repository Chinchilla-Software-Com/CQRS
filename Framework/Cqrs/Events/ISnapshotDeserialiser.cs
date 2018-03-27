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
	/// Deserialises <see cref="Snapshot"/> from a serialised state.
	/// </summary>
	public interface ISnapshotDeserialiser
	{
		/// <summary>
		/// Deserialise the provided <paramref name="eventData"/> into an <see cref="Snapshot"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to Deserialise.</param>
		Snapshot Deserialise(EventData eventData);
	}
}