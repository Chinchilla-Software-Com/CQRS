#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Domain;

namespace Cqrs.Snapshots
{
	/// <summary>
	/// Holds state information about an <see cref="IAggregateRoot{TAuthenticationToken}"/> for fast rehydration.
	/// </summary>
	[Serializable]
	public abstract class Snapshot
	{
		/// <summary>
		/// The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> this holds information about.
		/// </summary>
		[DataMember]
		public virtual Guid Id { get; set; }

		/// <summary>
		/// The version number the <see cref="IAggregateRoot{TAuthenticationToken}"/> was at when this snapshot was taken.
		/// </summary>
		[DataMember]
		public virtual int Version { get; set; }
	}
}