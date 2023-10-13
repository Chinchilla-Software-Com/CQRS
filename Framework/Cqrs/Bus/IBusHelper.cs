﻿#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Cqrs.Configuration;
using Cqrs.Messages;

namespace Cqrs.Bus
{
	/// <summary>
	/// A helper for command and event buses.
	/// </summary>
	public interface IBusHelper
	{
		/// <summary>
		/// Checks if a white-list or black-list approach is taken, then checks the <see cref="IConfigurationManager"/> to see if a key exists defining if the event is required or not.
		/// If the event is required and it cannot be resolved, an error will be raised.
		/// Otherwise the event will be marked as processed.
		/// </summary>
		/// <param name="messageType">The <see cref="Type"/> of the message being processed.</param>
		bool IsEventRequired(Type messageType);

		/// <summary>
		/// Checks if a white-list or black-list approach is taken, then checks the <see cref="IConfigurationManager"/> to see if a key exists defining if the event is required or not.
		/// If the event is required and it cannot be resolved, an error will be raised.
		/// Otherwise the event will be marked as processed.
		/// </summary>
		/// <param name="configurationKey">The configuration key to check.</param>
		bool IsEventRequired(string configurationKey);

		/// <summary>
		/// Checks if the private bus is required to send the message. Note, this does not imply the public bus is not required as well.
		/// </summary>
		/// <param name="messageType">The <see cref="Type"/> of the message being processed.</param>
		/// <returns>Null for unconfigured, True for private bus transmission, false otherwise.</returns>
		bool? IsPrivateBusRequired(Type messageType);

		/// <summary>
		/// Checks if the public bus is required to send the message. Note, this does not imply the public bus is not required as well.
		/// </summary>
		/// <param name="messageType">The <see cref="Type"/> of the message being processed.</param>
		/// <returns>Null for unconfigured, True for private bus transmission, false otherwise.</returns>
		bool? IsPublicBusRequired(Type messageType);

		/// <summary>
		/// Build a message handler that implements telemetry capturing as well as off thread handling.
		/// </summary>
#if NET40
		Action<TMessage>
#else
		Func<TMessage, Task>
#endif
			BuildTelemeteredActionHandler<TMessage, TAuthenticationToken>(ITelemetryHelper telemetryHelper,
#if NET40
			Action<TMessage>
#else
			Func<TMessage, Task>
#endif
				handler, bool holdMessageLock, string source)
			where TMessage : IMessage;

		/// <summary>
		/// Build a message handler that implements telemetry capturing as well as off thread handling.
		/// </summary>
#if NET40
		Action<TMessage>
#else
		Func<TMessage, Task>
#endif
			BuildActionHandler<TMessage>(

#if NET40
			Action<TMessage>
#else
			Func<TMessage, Task>
#endif
				handler, bool holdMessageLock)
			where TMessage : IMessage;

		/// <summary>
		/// Indicates if the message was received via the private bus or not. If false, this implies the public was use used.
		/// </summary>
		bool GetWasPrivateBusUsed();

		/// <summary>
		/// Set whether the message was received via the private bus or not. If false, this indicates the public was use used.
		/// </summary>
		bool SetWasPrivateBusUsed(bool wasPrivate);
	}
}