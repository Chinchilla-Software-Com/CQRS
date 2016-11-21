#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ServiceModel;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	/// <summary>
	/// Registers event or command handlers that listen and respond to events or commands.
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/Bus/HandlerRegistrar")]
	public interface IHandlerRegistrar
	{
		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		/// <remarks>
		/// In many cases the <paramref name="targetedType"/> will be the event handler class itself, what you actually want is the target of what is being updated
		/// </remarks>
		[OperationContract]
		void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage;

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		[OperationContract]
		void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage;
	}
}