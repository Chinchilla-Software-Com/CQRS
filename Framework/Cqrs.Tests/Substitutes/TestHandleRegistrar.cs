using System;
using System.Collections.Generic;
using Cqrs.Bus;
using Cqrs.Messages;

namespace Cqrs.Tests.Substitutes
{
	public class TestHandleRegistrar : IEventHandlerRegistrar, ICommandHandlerRegistrar
	{
		public static readonly IList<TestHandlerListItem> HandlerList = new List<TestHandlerListItem>();

		public void RegisterHandler<T>(Action<T> handler) where T : IMessage
		{
			HandlerList.Add(new TestHandlerListItem {Type = typeof(T),Handler = handler});
		}
	}

	public class TestHandlerListItem
	{
		public Type Type { get; set; }

		public dynamic Handler { get; set; }
	}
}
