#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading;
using cdmdotnet.Logging;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public interface IAzureBusHelper<TAuthenticationToken>
	{
		void PrepareCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>;

		bool PrepareAndValidateCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>;

		ICommand<TAuthenticationToken> ReceiveCommand(string messageBody, Func<ICommand<TAuthenticationToken>, bool?> receiveCommandHandler, string messageId, Action skippedAction = null, Action lockRefreshAction = null);

		/// <returns>
		/// True indicates the <paramref name="command"/> was successfully handled by a handler.
		/// False indicates the <paramref name="command"/> wasn't handled, but didn't throw an error, so by convention, that means it was skipped.
		/// Null indicates the command<paramref name="command"/> wasn't handled as it was already handled.
		/// </returns>
		bool? DefaultReceiveCommand(ICommand<TAuthenticationToken> command, RouteManager routeManager, string framework);

		void PrepareEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>;

		bool PrepareAndValidateEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>;

		IEvent<TAuthenticationToken> ReceiveEvent(string messageBody, Func<IEvent<TAuthenticationToken>, bool?> receiveCommandHandler, string messageId, Action skippedAction = null, Action lockRefreshAction = null);

		void RefreshLock(CancellationTokenSource brokeredMessageRenewCancellationTokenSource, BrokeredMessage message, string type = "message");

		/// <returns>
		/// True indicates the <paramref name="event"/> was successfully handled by a handler.
		/// False indicates the <paramref name="event"/> wasn't handled, but didn't throw an error, so by convention, that means it was skipped.
		/// Null indicates the <paramref name="event"/> wasn't handled as it was already handled.
		/// </returns>
		bool? DefaultReceiveEvent(IEvent<TAuthenticationToken> @event, RouteManager routeManager, string framework);

		void RegisterHandler<TMessage>(ITelemetryHelper telemetryHelper, RouteManager routeManger, Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage;
	}
}