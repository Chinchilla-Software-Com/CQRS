using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Infrastructure;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	public class InProcessBus<TAuthenticationToken> : ICommandSender<TAuthenticationToken>, IEventPublisher<TAuthenticationToken>, IHandlerRegistrar
	{
		private Dictionary<Type, List<Action<IMessage>>> Routes { get; set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		public InProcessBus(IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			AuthenticationTokenHelper = authenticationTokenHelper;
			Routes = new Dictionary<Type, List<Action<IMessage>>>();
		}

		public void RegisterHandler<TMessage>(Action<TMessage> handler)
			where TMessage : IMessage
		{
			List<Action<IMessage>> handlers;
			if(!Routes.TryGetValue(typeof(TMessage), out handlers))
			{
				handlers = new List<Action<IMessage>>();
				Routes.Add(typeof(TMessage), handlers);
			}
			handlers.Add(DelegateAdjuster.CastArgument<IMessage, TMessage>(x => handler(x)));
		}

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();

			List<Action<IMessage>> handlers; 
			if (Routes.TryGetValue(typeof(TCommand), out handlers))
			{
				if (handlers.Count != 1)
					throw new InvalidOperationException("Cannot send to more than one handler");
				handlers.Single()(command);
			}
			else
			{
				throw new InvalidOperationException("No handler registered");
			}
		}

		public void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			@event.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();

			List<Action<IMessage>> handlers; 
			if (!Routes.TryGetValue(@event.GetType(), out handlers)) return;
			foreach(Action<IMessage> handler in handlers)
				handler(@event);
			
		}
	}
}
