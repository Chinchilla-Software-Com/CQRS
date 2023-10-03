#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading;
using Chinchilla.Logging;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
using Microsoft.Azure.ServiceBus.Core;
using BrokeredMessage = Microsoft.Azure.ServiceBus.Message;
#else
using Microsoft.ServiceBus.Messaging;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// A helper for Azure Service Bus and Event Hub.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface IAzureBusHelper<TAuthenticationToken>
	{
		/// <summary>
		/// Prepares a <see cref="ICommand{TAuthenticationToken}"/> to be sent specifying the framework it is sent via.
		/// </summary>
		/// <typeparam name="TCommand">The <see cref="Type"/> of<see cref="ICommand{TAuthenticationToken}"/> being sent.</typeparam>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> to send.</param>
		/// <param name="framework">The framework the <paramref name="command"/> is being sent from.</param>
		void PrepareCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Prepares and validates a <see cref="ICommand{TAuthenticationToken}"/> to be sent specifying the framework it is sent via.
		/// </summary>
		/// <typeparam name="TCommand">The <see cref="Type"/> of<see cref="ICommand{TAuthenticationToken}"/> being sent.</typeparam>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> to send.</param>
		/// <param name="framework">The framework the <paramref name="command"/> is being sent from.</param>
		bool PrepareAndValidateCommand<TCommand>(TCommand command, string framework)
			where TCommand : ICommand<TAuthenticationToken>;

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		/// <summary>
		/// Deserialises and processes the <paramref name="messageBody"/> received from the network through the provided <paramref name="receiveCommandHandler"/>.
		/// </summary>
		/// <param name="client">The channel the message was received on.</param>
		/// <param name="messageBody">A serialised <see cref="IMessage"/>.</param>
		/// <param name="receiveCommandHandler">The handler method that will process the <see cref="ICommand{TAuthenticationToken}"/>.</param>
		/// <param name="messageId">The network id of the <see cref="IMessage"/>.</param>
		/// <param name="signature">The signature of the <see cref="IMessage"/>.</param>
		/// <param name="signingTokenConfigurationKey">The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.</param>
		/// <param name="skippedAction">The <see cref="Action"/> to call when the <see cref="ICommand{TAuthenticationToken}"/> is being skipped.</param>
		/// <param name="lockRefreshAction">The <see cref="Action"/> to call to refresh the network lock.</param>
		/// <returns>The <see cref="ICommand{TAuthenticationToken}"/> that was processed.</returns>
#else
		/// <summary>
		/// Deserialises and processes the <paramref name="messageBody"/> received from the network through the provided <paramref name="receiveCommandHandler"/>.
		/// </summary>
		/// <param name="serviceBusReceiver">The channel the message was received on.</param>
		/// <param name="messageBody">A serialised <see cref="IMessage"/>.</param>
		/// <param name="receiveCommandHandler">The handler method that will process the <see cref="ICommand{TAuthenticationToken}"/>.</param>
		/// <param name="messageId">The network id of the <see cref="IMessage"/>.</param>
		/// <param name="signature">The signature of the <see cref="IMessage"/>.</param>
		/// <param name="signingTokenConfigurationKey">The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.</param>
		/// <param name="skippedAction">The <see cref="Action"/> to call when the <see cref="ICommand{TAuthenticationToken}"/> is being skipped.</param>
		/// <param name="lockRefreshAction">The <see cref="Action"/> to call to refresh the network lock.</param>
		/// <returns>The <see cref="ICommand{TAuthenticationToken}"/> that was processed.</returns>
#endif
		ICommand<TAuthenticationToken> ReceiveCommand(
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
			IMessageReceiver client
#else
			IMessageReceiver serviceBusReceiver
#endif
			, string messageBody, Func<ICommand<TAuthenticationToken>, bool?> receiveCommandHandler, string messageId, string signature, string signingTokenConfigurationKey, Action skippedAction = null, Action lockRefreshAction = null);

		/// <summary>
		/// The default command handler that
		/// check if the <see cref="ICommand{TAuthenticationToken}"/> has already been processed by this framework,
		/// checks if the <see cref="ICommand{TAuthenticationToken}"/> is required,
		/// finds the handler from the provided <paramref name="routeManager"/>.
		/// </summary>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> to process.</param>
		/// <param name="routeManager">The <see cref="RouteManager"/> to get the <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> from.</param>
		/// <param name="framework">The current framework.</param>
		/// <returns>
		/// True indicates the <paramref name="command"/> was successfully handled by a handler.
		/// False indicates the <paramref name="command"/> wasn't handled, but didn't throw an error, so by convention, that means it was skipped.
		/// Null indicates the command<paramref name="command"/> wasn't handled as it was already handled.
		/// </returns>
		bool? DefaultReceiveCommand(ICommand<TAuthenticationToken> command, RouteManager routeManager, string framework);

		/// <summary>
		/// Prepares an <see cref="IEvent{TAuthenticationToken}"/> to be sent specifying the framework it is sent via.
		/// </summary>
		/// <typeparam name="TEvent">The <see cref="Type"/> of<see cref="IEvent{TAuthenticationToken}"/> being sent.</typeparam>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to send.</param>
		/// <param name="framework">The framework the <paramref name="event"/> is being sent from.</param>
		void PrepareEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>;

		/// <summary>
		/// Prepares and validates an <see cref="IEvent{TAuthenticationToken}"/> to be sent specifying the framework it is sent via.
		/// </summary>
		/// <typeparam name="TEvent">The <see cref="Type"/> of<see cref="IEvent{TAuthenticationToken}"/> being sent.</typeparam>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to send.</param>
		/// <param name="framework">The framework the <paramref name="event"/> is being sent from.</param>
		bool PrepareAndValidateEvent<TEvent>(TEvent @event, string framework)
			where TEvent : IEvent<TAuthenticationToken>;

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		/// <summary>
		/// Deserialises and processes the <paramref name="messageBody"/> received from the network through the provided <paramref name="receiveEventHandler"/>.
		/// </summary>
		/// <param name="client">The channel the message was received on.</param>
		/// <param name="messageBody">A serialised <see cref="IMessage"/>.</param>
		/// <param name="receiveEventHandler">The handler method that will process the <see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="messageId">The network id of the <see cref="IMessage"/>.</param>
		/// <param name="signature">The signature of the <see cref="IMessage"/>.</param>
		/// <param name="signingTokenConfigurationKey">The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.</param>
		/// <param name="skippedAction">The <see cref="Action"/> to call when the <see cref="IEvent{TAuthenticationToken}"/> is being skipped.</param>
		/// <param name="lockRefreshAction">The <see cref="Action"/> to call to refresh the network lock.</param>
		/// <returns>The <see cref="IEvent{TAuthenticationToken}"/> that was processed.</returns>
#else
		/// <summary>
		/// Deserialises and processes the <paramref name="messageBody"/> received from the network through the provided <paramref name="receiveEventHandler"/>.
		/// </summary>
		/// <param name="serviceBusReceiver">The channel the message was received on.</param>
		/// <param name="messageBody">A serialised <see cref="IMessage"/>.</param>
		/// <param name="receiveEventHandler">The handler method that will process the <see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="messageId">The network id of the <see cref="IMessage"/>.</param>
		/// <param name="signature">The signature of the <see cref="IMessage"/>.</param>
		/// <param name="signingTokenConfigurationKey">The configuration key for the signing token as used by <see cref="IConfigurationManager"/>.</param>
		/// <param name="skippedAction">The <see cref="Action"/> to call when the <see cref="IEvent{TAuthenticationToken}"/> is being skipped.</param>
		/// <param name="lockRefreshAction">The <see cref="Action"/> to call to refresh the network lock.</param>
		/// <returns>The <see cref="IEvent{TAuthenticationToken}"/> that was processed.</returns>
#endif
		IEvent<TAuthenticationToken> ReceiveEvent(
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
			IMessageReceiver client
#else
			IMessageReceiver serviceBusReceiver
#endif
			, string messageBody, Func<IEvent<TAuthenticationToken>, bool?> receiveEventHandler, string messageId, string signature, string signingTokenConfigurationKey, Action skippedAction = null, Action lockRefreshAction = null);

		/// <summary>
		/// Refreshes the network lock.
		/// </summary>
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		void RefreshLock(IMessageReceiver client, CancellationTokenSource brokeredMessageRenewCancellationTokenSource, BrokeredMessage message, string type = "message");
#else
		void RefreshLock(CancellationTokenSource brokeredMessageRenewCancellationTokenSource, BrokeredMessage message, string type = "message");
#endif

		/// <summary>
		/// The default event handler that
		/// check if the <see cref="IEvent{TAuthenticationToken}"/> has already been processed by this framework,
		/// checks if the <see cref="IEvent{TAuthenticationToken}"/> is required,
		/// finds the handler from the provided <paramref name="routeManager"/>.
		/// </summary>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to process.</param>
		/// <param name="routeManager">The <see cref="RouteManager"/> to get the <see cref="IEventHandler{TAuthenticationToken,TCommand}"/> from.</param>
		/// <param name="framework">The current framework.</param>
		/// <returns>
		/// True indicates the <paramref name="event"/> was successfully handled by a handler.
		/// False indicates the <paramref name="event"/> wasn't handled, but didn't throw an error, so by convention, that means it was skipped.
		/// Null indicates the <paramref name="event"/> wasn't handled as it was already handled.
		/// </returns>
		bool? DefaultReceiveEvent(IEvent<TAuthenticationToken> @event, RouteManager routeManager, string framework);

		/// <summary>
		/// Manually registers the provided <paramref name="handler"/> 
		/// on the provided <paramref name="routeManger"/>
		/// </summary>
		/// <typeparam name="TMessage">The <see cref="Type"/> of <see cref="IMessage"/> the <paramref name="handler"/> can handle.</typeparam>
		void RegisterHandler<TMessage>(ITelemetryHelper telemetryHelper, RouteManager routeManger, Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage;

		/// <summary>
		/// Register an event handler that will listen and respond to all events.
		/// </summary>
		void RegisterGlobalEventHandler<TMessage>(ITelemetryHelper telemetryHelper, RouteManager routeManger, Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage;
	}
}