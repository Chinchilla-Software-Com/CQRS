#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Messages;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusReceiver<TAuthenticationToken>
		: AzureCommandBus<TAuthenticationToken>
		, ICommandHandlerRegistrar
		, ICommandReceiver<TAuthenticationToken>
	{
		// ReSharper disable StaticMemberInGenericType
		protected static RouteManager Routes { get; private set; }

		protected static long CurrentHandles { get; set; }
		// ReSharper restore StaticMemberInGenericType

		protected ITelemetryHelper TelemetryHelper { get; private set; }

		static AzureCommandBusReceiver()
		{
			Routes = new RouteManager();
		}

		public AzureCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.CommandBus.Receiver.UseApplicationInsightTelemetryHelper");
		}

		public virtual void RegisterHandler<TMessage>(Action<TMessage> handler, Type targetedType, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			AzureBusHelper.RegisterHandler(TelemetryHelper, Routes, handler, targetedType, holdMessageLock);
		}

		/// <summary>
		/// Register an event or command handler that will listen and respond to events or commands.
		/// </summary>
		public void RegisterHandler<TMessage>(Action<TMessage> handler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			RegisterHandler(handler, null, holdMessageLock);
		}

		protected virtual void ReceiveCommand(BrokeredMessage message)
		{
			IDictionary<string, string> telemetryProperties = new Dictionary<string, string> { { "Type", "Azure/Servicebus" } };
			TelemetryHelper.TrackMetric("Cqrs/Handle/Command", CurrentHandles++, telemetryProperties);
			var brokeredMessageRenewCancellationTokenSource = new CancellationTokenSource();
			try
			{
				Logger.LogDebug(string.Format("A command message arrived with the id '{0}'.", message.MessageId));
				string messageBody = message.GetBody<string>();


				AzureBusHelper.ReceiveCommand(messageBody, ReceiveCommand,
					string.Format("id '{0}'", message.MessageId),
					() =>
					{
						// Remove message from queue
						message.Complete();
						Logger.LogDebug(string.Format("A command message arrived with the id '{0}' but processing was skipped due to command settings.", message.MessageId));
					},
					() =>
					{
						Task.Factory.StartNewSafely(() =>
						{
							long loop = long.MinValue;
							while (!brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
							{
								//Based on LockedUntilUtc property to determine if the lock expires soon
								if (DateTime.UtcNow > message.LockedUntilUtc.AddSeconds(-10))
								{
									// If so, renew the lock
									for (int i = 0; i < 10; i++)
									{
										try
										{
											message.RenewLock();
											break;
										}
										catch { }
									}
								}

								if (loop++ % 5 == 0)
									Thread.Yield();
								else
									Thread.Sleep(500);
								if (loop == long.MaxValue)
									loop = long.MinValue;
							}
						}, brokeredMessageRenewCancellationTokenSource.Token);
					}
				);

				// Remove message from queue
				message.Complete();
				Logger.LogDebug(string.Format("A command message arrived and was processed with the id '{0}'.", message.MessageId));
			}
			catch (Exception exception)
			{
				TelemetryHelper.TrackException(exception, null, telemetryProperties);
				// Indicates a problem, unlock message in queue
				Logger.LogError(string.Format("A command message arrived with the id '{0}' but failed to be process.", message.MessageId), exception: exception);
				message.Abandon();
			}
			finally
			{
				// Cancel the lock of renewing the task
				brokeredMessageRenewCancellationTokenSource.Cancel();
				TelemetryHelper.TrackMetric("Cqrs/Handle/Command", CurrentHandles--, telemetryProperties);
			}
		}

		public virtual void ReceiveCommand(ICommand<TAuthenticationToken> command)
		{
			AzureBusHelper.DefaultReceiveCommand(command, Routes, "Azure-ServiceBus");
		}

		#region Implementation of ICommandReceiver

		public void Start()
		{
			InstantiateReceiving();

			// Configure the callback options
			OnMessageOptions options = new OnMessageOptions
			{
				AutoComplete = false,
				AutoRenewTimeout = TimeSpan.FromMinutes(1)
			};

			// Callback to handle received messages
			RegisterReceiverMessageHandler(ReceiveCommand, options);
		}

		#endregion
	}
}