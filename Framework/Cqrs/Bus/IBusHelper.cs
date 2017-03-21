#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Messages;

namespace Cqrs.Bus
{
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
		/// Build a message handler that implements telemetry capturing as well as off thread handling.
		/// </summary>
		Action<TMessage> BuildTelemeteredActionHandler<TMessage, TAuthenticationToken>(ITelemetryHelper telemetryHelper, Action<TMessage> handler, bool holdMessageLock, string source)
			where TMessage : IMessage;

		/// <summary>
		/// Build a message handler that implements telemetry capturing as well as off thread handling.
		/// </summary>
		Action<TMessage> BuildActionHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock)
			where TMessage : IMessage;
	}
}