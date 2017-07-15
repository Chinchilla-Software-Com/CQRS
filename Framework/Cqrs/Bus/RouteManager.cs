#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
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
		/// <summary>
		/// The <see cref="Route"/> to execute per <see cref="Type"/>
		/// </summary>
		protected IDictionary<Type, Route> Routes { get; private set; }

		/// <summary>
		/// A <see cref="Route"/> to execute for all <see cref="IEvent{TAuthenticationToken}"/>
		/// </summary>
		public Route GlobalEventRoute { get; private set; }

		private static Type CommandType { get; set; }

		private static Type EventType { get; set; }

		public RouteManager()
		{
			Routes = new Dictionary<Type, Route>();
			GlobalEventRoute = new Route();
		}

		static RouteManager()
		{
			CommandType = typeof (ICommand<>);
			EventType = typeof (IEvent<>);
		}

		#region Implementation of IHandlerRegistrar

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
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
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		/// <summary>
		/// Register an event handler that will listen and respond to all events.
		/// </summary>
		public void RegisterGlobalEventHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true) where TMessage : IMessage
		{
			GlobalEventRoute.Handlers.Add
			(
				new RouteHandlerDelegate
				{
					Delegate = DelegateAdjuster.CastArgument<IMessage, TMessage>(x => handler(x)),
					TargetedType = null
				}
			);
		}

		#endregion

		public RouteHandlerDelegate GetSingleHandler<TMessage>(bool throwExceptionOnNoRouteHandlers = true)
			where TMessage : IMessage
		{
			Route route;
			Type messageType = typeof(TMessage);
			bool isACommand = IsACommand(messageType);

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
			bool isACommand = IsACommand(messageType);

			if (Routes.TryGetValue(messageType, out route))
			{
				if (route.Handlers != null)
				{
					if (route.Handlers.Count > 1)
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
			Type messageType = message.GetType();
			bool isACommand = IsACommand(messageType);
			bool isAnEvent = IsAnEvent(messageType);

			var routeHandlers = new List<RouteHandlerDelegate>();
			if (isAnEvent && GlobalEventRoute.Handlers != null)
				routeHandlers.AddRange(GlobalEventRoute.Handlers);

			Route route;
			if (Routes.TryGetValue(messageType, out route))
			{
				routeHandlers.AddRange(route.Handlers);
				return routeHandlers;
			}

			if (throwExceptionOnNoRouteHandlers)
			{
				if (isACommand)
					throw new NoCommandHandlerRegisteredException(messageType);
				if (isAnEvent)
					throw new NoEventHandlerRegisteredException(messageType);
				throw new NoHandlerRegisteredException(messageType);
			}

			return Enumerable.Empty<RouteHandlerDelegate>();
		}

		protected virtual bool IsACommand<TMessage>(TMessage message)
		{
			Type messageType = message.GetType();
			return IsACommand(messageType);
		}

		protected virtual bool IsACommand(Type messageType)
		{
			bool isACommand = false;
			Type messageCommandInterface = messageType.GetInterfaces().FirstOrDefault(type => type.FullName.StartsWith(CommandType.FullName));
			if (messageCommandInterface != null)
			{
				Type[] genericArguments = messageCommandInterface.GetGenericArguments();
				if (genericArguments.Length == 1)
					isACommand = CommandType.MakeGenericType(genericArguments.Single()).IsAssignableFrom(messageType);
			}

			return isACommand;
		}

		protected virtual bool IsAnEvent<TMessage>(TMessage message)
		{
			Type messageType = message.GetType();
			return IsACommand(messageType);
		}

		protected virtual bool IsAnEvent(Type messageType)
		{
			bool isAnEvent = false;
			Type messageCommandInterface = messageType.GetInterfaces().FirstOrDefault(type => type.FullName.StartsWith(EventType.FullName));
			if (messageCommandInterface != null)
			{
				Type[] genericArguments = messageCommandInterface.GetGenericArguments();
				if (genericArguments.Length == 1)
					isAnEvent = EventType.MakeGenericType(genericArguments.Single()).IsAssignableFrom(messageType);
			}

			return isAnEvent;
		}
	}
}