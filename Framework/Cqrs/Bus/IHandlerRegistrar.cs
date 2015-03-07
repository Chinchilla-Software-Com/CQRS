using System;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	/// <summary>
	/// Registers event or command handlers that listen and respond to events or commands.
	/// </summary>
	public interface IHandlerRegistrar
	{
		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		void RegisterHandler<TMessage>(Action<TMessage> handler)
			where TMessage : IMessage;
	}
}