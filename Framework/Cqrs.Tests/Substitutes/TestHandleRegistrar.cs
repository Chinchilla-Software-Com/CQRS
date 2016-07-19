using System;
using System.Collections.Generic;
using Cqrs.Bus;
using Cqrs.Messages;

namespace Cqrs.Tests.Substitutes
{
	public class TestHandleRegistrar : IEventHandlerRegistrar, ICommandHandlerRegistrar
	{
		public static readonly IList<TestHandlerListItem> HandlerList = new List<TestHandlerListItem>();

		public void RegisterHandler<T>(Action<T> handler, Type targetedType, bool holdMessageLock = true)
			where T : IMessage
		{
			HandlerList.Add(new TestHandlerListItem {Type = typeof(T),Handler = handler});
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}
	}

	public class TestHandlerListItem
	{
		public Type Type { get; set; }

		public dynamic Handler { get; set; }
	}
}
