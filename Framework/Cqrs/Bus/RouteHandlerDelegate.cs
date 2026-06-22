#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Configuration;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	/// <summary>
	/// Information about a <see cref="Route"/> delegate.
	/// </summary>
	/// <remarks>Remarkably similar to <see cref="HandlerDelegate"/></remarks>
	public class RouteHandlerDelegate : HandlerDelegate<IMessage>
	{
	}
}