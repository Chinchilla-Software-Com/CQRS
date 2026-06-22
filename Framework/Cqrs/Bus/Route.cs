#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace Cqrs.Bus
{
	/// <summary>
	/// A collection of <see cref="RouteHandlerDelegate"/>.
	/// </summary>
	public class Route
	{
		/// <summary>
		/// Gets or sets the <see cref="RouteHandlerDelegate"/>.
		/// </summary>
		public IList<RouteHandlerDelegate> Handlers { get; set; }
	}
}