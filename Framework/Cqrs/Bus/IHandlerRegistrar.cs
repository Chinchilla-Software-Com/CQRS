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
	[ServiceContract(Namespace = "http://cqrs.co.nz/Bus/HandlerRegistrar")]
	public interface IHandlerRegistrar
	{
		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		[OperationContract]
		void RegisterHandler<TMessage>(Action<TMessage> handler)
			where TMessage : IMessage;
	}
}