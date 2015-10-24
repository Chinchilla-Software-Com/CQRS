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

		#endregion

		public RouteHandlerDelegate GetSingleHandler<TMessage>(bool throwExceptionOnNoRouteHandlers = true)
			where TMessage : IMessage
		{
			Route route;
			if (Routes.TryGetValue(typeof(TMessage), out route))
			{
				if (route.Handlers == null || route.Handlers.Count != 1)
					throw new InvalidOperationException("Cannot send to more than one handler.");
				return route.Handlers.Single();
			}

			if (throwExceptionOnNoRouteHandlers)
				throw new InvalidOperationException("No handler registered.");

			return null;
		}

		public RouteHandlerDelegate GetSingleHandler<TMessage>(TMessage message, bool throwExceptionOnNoRouteHandlers = true)
			where TMessage : IMessage
		{
			Route route;
			if (Routes.TryGetValue(message.GetType(), out route))
			{
				if (route.Handlers == null || route.Handlers.Count != 1)
					throw new InvalidOperationException("Cannot send to more than one handler.");
				return route.Handlers.Single();
			}

			if (throwExceptionOnNoRouteHandlers)
				throw new InvalidOperationException("No handler registered.");

			return null;
		}

		public IEnumerable<RouteHandlerDelegate> GetHandlers<TMessage>(TMessage message, bool throwExceptionOnNoRouteHandlers = true)
			where TMessage : IMessage
		{
			Route route;
			if (Routes.TryGetValue(message.GetType(), out route))
				return route.Handlers;

			if (throwExceptionOnNoRouteHandlers)
				throw new InvalidOperationException("No handlers registered.");

			return null;
		}
	}
}