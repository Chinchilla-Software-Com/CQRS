#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Snapshots
{
	public abstract class Snapshot
	{
		public virtual Guid Id { get; set; }

		public virtual int Version { get; set; }
	}
}