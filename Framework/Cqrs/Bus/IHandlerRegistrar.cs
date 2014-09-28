using System;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	public interface IHandlerRegistrar
	{
		void RegisterHandler<TMessage>(Action<TMessage> handler)
			where TMessage : IMessage;
	}
}