#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.ServiceModel;

namespace Cqrs.Bus
{
	/// <summary>
	/// Registers event handlers that listen and respond to events.
	/// </summary>
	[ServiceContract(Namespace = "http://cqrs.co.nz/Bus/EventHandlerRegistrar")]
	public interface IEventHandlerRegistrar : IHandlerRegistrar
	{
	}
}