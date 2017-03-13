#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Azure.ServiceBus
{
	public interface IAzureBusHelper<TAuthenticationToken>
	{
		void PrepareCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>;

		bool PrepareAndValidateCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>;

		ICommand<TAuthenticationToken> ReceiveCommand(string messageBody, Action<ICommand<TAuthenticationToken>> receiveCommandHandler, string messageId, Action skippedAction = null, Action lockRefreshAction = null);

		void DefaultReceiveCommand(ICommand<TAuthenticationToken> command, RouteManager routeManager, string framework);

		void PrepareEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>;

		bool PrepareAndValidateEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>;

		IEvent<TAuthenticationToken> ReceiveEvent(string messageBody, Action<IEvent<TAuthenticationToken>> receiveCommandHandler, string messageId, Action skippedAction = null, Action lockRefreshAction = null);


		void DefaultReceiveEvent(IEvent<TAuthenticationToken> @event, RouteManager routeManager, string framework);

		void RegisterHandler<TMessage>(ITelemetryHelper telemetryHelper, RouteManager routeManger, Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage;
	}
}