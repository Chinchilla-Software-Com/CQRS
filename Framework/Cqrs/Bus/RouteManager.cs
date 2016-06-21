#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Infrastructure;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	public class RouteManager
		: IHandlerRegistrar
	{
		protected IDictionary<Type, Route> Routes { get; private set; }

		public RouteManager()
		{
			Routes = new Dictionary<Type, Route>();
		}

		#region Implementation of IHandlerRegistrar

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType)
			where TMessage : IMessage
		{
			Route route;
			if (!Routes.TryGetValue(typeof(TMessage), out route))
			{
				route = new Route
				{
					Handlers = new List<RouteHandlerDelegate>()
				};
				Routes.Add(typeof(TMessage), route);
			}
			route.Handlers.Add
			(
				new RouteHandlerDelegate
				{
					Delegate = DelegateAdjuster.CastArgument<IMessage, TMessage>(x => handler(x)),
					TargetedType = targetedType
				}
			);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null);
		}

		#endregion

		public RouteHandlerDelegate GetSingleHandler<TMessage>(bool throwExceptionOnNoRouteHandlers = true)
			where TMessage : IMessage
		{
			Route route;
			Type messageType = typeof(TMessage);
			bool isACommand = typeof(ICommand<>).IsAssignableFrom(messageType);

			if (Routes.TryGetValue(typeof(TMessage), out route))
			{
				if (route.Handlers == null || route.Handlers.Count != 1)
				{
					if (isACommand)
						throw new MultipleCommandHandlersRegisteredException(messageType);
					throw new InvalidOperationException("Cannot send to more than one handler.");
				}
				return route.Handlers.Single();
			}

			if (throwExceptionOnNoRouteHandlers)
			{
				if (isACommand)
					throw new NoCommandHandlerRegisteredException(messageType);
				throw new InvalidOperationException("No handler registered.");
			}

			return null;
		}

		public RouteHandlerDelegate GetSingleHandler<TMessage>(TMessage message, bool throwExceptionOnNoRouteHandlers = true)
			where TMessage : IMessage
		{
			Route route;
			Type messageType = message.GetType();
			bool isACommand = typeof (ICommand<>).IsAssignableFrom(messageType);

			if (Routes.TryGetValue(messageType, out route))
			{
				if (route.Handlers != null)
				{
					if (route.Handlers.Count != 1)
					{
						if (isACommand)
							throw new MultipleCommandHandlersRegisteredException(messageType);
						throw new InvalidOperationException("Cannot send to more than one handler.");
					}
					if (route.Handlers.Count == 1)
						return route.Handlers.Single();
				}
			}

			if (throwExceptionOnNoRouteHandlers)
			{
				if (isACommand)
					throw new NoCommandHandlerRegisteredException(messageType);
				throw new InvalidOperationException("No handler registered.");
			}

			return null;
		}

		public IEnumerable<RouteHandlerDelegate> GetHandlers<TMessage>(TMessage message, bool throwExceptionOnNoRouteHandlers = true)
			where TMessage : IMessage
		{
			Route route;
			Type messageType = message.GetType();
			if (Routes.TryGetValue(messageType, out route))
				return route.Handlers;

			if (throwExceptionOnNoRouteHandlers)
			{
				bool isACommand = typeof(ICommand<>).IsAssignableFrom(messageType);
				bool isAnEvent = typeof(IEvent<>).IsAssignableFrom(messageType);

				if (isACommand)
					throw new NoCommandHandlerRegisteredException(messageType);
				if (isAnEvent)
					throw new NoEventHandlerRegisteredException(messageType);
				throw new InvalidOperationException("No handler(s) registered.");
			}

			return Enumerable.Empty<RouteHandlerDelegate>();
		}
	}
}