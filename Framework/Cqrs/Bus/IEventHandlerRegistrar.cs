#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ServiceModel;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	/// <summary>
	/// Registers event handlers that listen and respond to events.
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/Bus/EventHandlerRegistrar")]
	public interface IEventHandlerRegistrar : IHandlerRegistrar
	{
		/// <summary>
		/// Register an event handler that will listen and respond to all events.
		/// </summary>
		[OperationContract]
		void RegisterGlobalEventHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage;
	}
}