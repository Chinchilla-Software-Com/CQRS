#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	/// <summary>
	/// Registers event or command handlers that listen and respond to events or commands.
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/Bus/HandlerRegistrar")]
	public interface IAsyncHandlerRegistrar
	{
		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		/// <remarks>
		/// In many cases the <paramref name="targetedType"/> will be the handler class itself, what you actually want is the target of what is being updated.
		/// </remarks>
		[OperationContract]
		Task RegisterHandlerAsync<TMessage>(Func<TMessage, Task> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage;

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		[OperationContract]
		Task RegisterHandlerAsync<TMessage>(Func<TMessage, Task> handler, bool holdMessageLock = true)
			where TMessage : IMessage;
	}
}