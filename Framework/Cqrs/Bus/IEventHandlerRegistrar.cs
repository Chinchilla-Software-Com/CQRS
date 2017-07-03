#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.ServiceModel;

namespace Cqrs.Bus
{
	/// <summary>
	/// Registers event handlers that listen and respond to events.
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/Bus/EventHandlerRegistrar")]
	public interface IEventHandlerRegistrar : IHandlerRegistrar
	{
	}
}