#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace Cqrs.Bus
{
	public class Route
	{
		public IList<RouteHandlerDelegate> Handlers { get; set; }
	}
}