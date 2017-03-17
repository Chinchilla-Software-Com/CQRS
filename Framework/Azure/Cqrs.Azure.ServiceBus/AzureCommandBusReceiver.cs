#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		protected virtual string FilterKeyConfigurationKey
		{
			get { return "Cqrs.Azure.CommandBus.TopicName.SubscriptionName.Filter"; }
		}

		// ReSharper disable StaticMemberInGenericType
		protected static RouteManager Routes { get; private set; }

		protected static long CurrentHandles { get; set; }
		// ReSharper restore StaticMemberInGenericType

		static AzureCommandBusReceiver()
		{
			Routes = new RouteManager();
		}

		public AzureCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, BusHelper busHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, false)
		{
			TelemetryHelper = configurationManager.CreateTelemetryHelper("Cqrs.Azure.CommandBus.Receiver.UseApplicationInsightTelemetryHelper", correlationIdHelper);
		}

		#region Overrides of AzureServiceBus<TAuthenticationToken>

		protected override void InstantiateReceiving(IDictionary<int, SubscriptionClient> serviceBusReceivers, string topicName, string topicSubscriptionName)
		{
			base.InstantiateReceiving(serviceBusReceivers, topicName, topicSubscriptionName);

			string filter = ConfigurationManager.GetSetting(FilterKeyConfigurationKey);
			if (!string.IsNullOrWhiteSpace(filter))
			{
				SubscriptionClient client = serviceBusReceivers[0];
				RuleDescription ruleDescription = new RuleDescription
					(
					"CqrsConfiguredFilter",
					new SqlFilter(filter)
					);
				client.AddRuleAsync(ruleDescription);
			}
		}

		#endregion

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


				ICommand<TAuthenticationToken> command = AzureBusHelper.ReceiveCommand(messageBody, ReceiveCommand,
					string.Format("id '{0}'", message.MessageId),
					() =>
					{
						// Remove message from queue
						try
						{
							message.Complete();
						}
						catch (MessageLockLostException exception)
						{
							throw new MessageLockLostException(string.Format("The lock supplied for the skipped message '{0}' is invalid.", message.MessageId), exception);
						}
						Logger.LogDebug(string.Format("A command message arrived with the id '{0}' but processing was skipped due to command settings.", message.MessageId));
					},
					() =>
					{
						Task.Factory.StartNewSafely(() =>
						{
							long loop = long.MinValue;
							while (!brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
							{
								// Based on LockedUntilUtc property to determine if the lock expires soon
								// We lock for 45 seconds to ensure any thread based issues are mitigated.
								if (DateTime.UtcNow > message.LockedUntilUtc.AddSeconds(-45))
								{
									// If so, renew the lock
									for (int i = 0; i < 10; i++)
									{
										try
										{
											message.RenewLock();
											try
											{
												Logger.LogDebug(string.Format("Renewed the lock on command '{0}'.", message.MessageId));
											}
											catch
											{
												Trace.TraceError("Renewed the lock on command '{0}'.", message.MessageId);
											}

											break;
										}
										catch (ObjectDisposedException)
										{
											return;
										}
										catch (MessageLockLostException exception)
										{
											try
											{
												Logger.LogWarning(string.Format("Renewing the lock on command '{0}' failed as the message lock was lost.", message.MessageId), exception: exception);
											}
											catch
											{
												Trace.TraceError("Renewing the lock on command '{0}' failed as the message lock was lost.\r\n{1}", message.MessageId, exception.Message);
											}
											return;
										}
										catch (Exception exception)
										{
											try
											{
												Logger.LogWarning(string.Format("Renewing the lock on command '{0}' failed.", message.MessageId), exception: exception);
											}
											catch
											{
												Trace.TraceError("Renewing the lock on command '{0}' failed.\r\n{1}", message.MessageId, exception.Message);
											}
											if (i == 9)
												return;
										}
									}
								}

								if (loop++ % 5 == 0)
									Thread.Yield();
								else
									Thread.Sleep(500);
								if (loop == long.MaxValue)
									loop = long.MinValue;
							}
							try
							{
								brokeredMessageRenewCancellationTokenSource.Dispose();
							}
							catch (ObjectDisposedException) { }
						}, brokeredMessageRenewCancellationTokenSource.Token);
					}
				);

				// Remove message from queue
				try
				{
					message.Complete();
				}
				catch (MessageLockLostException exception)
				{
					throw new MessageLockLostException(string.Format("The lock supplied for command '{0}' of type {1} is invalid.", command.Id, command.GetType().Name), exception);
				}
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