#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using System.Threading;

using Azure.Messaging.ServiceBus;
using Chinchilla.Logging;
using Cqrs.Azure.ServiceBus;

namespace Cqrs.Azure.Functions.ServiceBus
{
	internal static class ServiceBusExtensions
	{
		/// <summary>
		/// Refreshes the network lock.
		/// </summary>
		public static async void RefreshLockAsync<TAuthenticationToken>(this IAzureBusHelper<TAuthenticationToken> azureBusHelper, ILogger logger, ServiceBusReceiver client, CancellationTokenSource brokeredMessageRenewCancellationTokenSource, ServiceBusReceivedMessage message, string type = "message")
		{
			// The capturing of ObjectDisposedException is because even the properties can throw it.
			try
			{
				object value;
				string typeName = null;
				if (message.TryGetUserPropertyValue("Type", out value))
					typeName = value.ToString();

				long loop = long.MinValue;
				while (!brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
				{
					// Based on LockedUntilUtc property to determine if the lock expires soon
					// We lock for 45 seconds to ensure any thread based issues are mitigated.
					if (DateTime.UtcNow > message.ExpiresAt.AddSeconds(-45))
					{
						// If so, renew the lock
						for (int i = 0; i < 10; i++)
						{
							try
							{
								if (brokeredMessageRenewCancellationTokenSource.Token.IsCancellationRequested)
									return;
								await client.RenewMessageLockAsync(message, brokeredMessageRenewCancellationTokenSource.Token);
								try
								{
									logger.LogDebug(string.Format("Renewed the {2} lock on {1} '{0}'.", message.MessageId, type, typeName), "Cqrs.Azure.ServiceBus.AzureBusHelper.RefreshLockAsync");
								}
								catch
								{
									Trace.TraceError("Renewed the {2} lock on {1} '{0}'.", message.MessageId, type, typeName);
								}

								break;
							}
							catch (ObjectDisposedException)
							{
								return;
							}
							catch (ServiceBusException exception)
							{
								try
								{
									logger.LogWarning(string.Format("Renewing the {2} lock on {1} '{0}' failed as the message lock was lost.", message.MessageId, type, typeName), "Cqrs.Azure.ServiceBus.AzureBusHelper.RefreshLockAsync", exception: exception);
								}
								catch
								{
									Trace.TraceError("Renewing the {2} lock on {1} '{0}' failed as the message lock was lost.\r\n{3}", message.MessageId, type, typeName, exception.Message);
								}
								return;
							}
							catch (Exception exception)
							{
								try
								{
									logger.LogWarning(string.Format("Renewing the {2} lock on {1} '{0}' failed.", message.MessageId, type, typeName), "Cqrs.Azure.ServiceBus.AzureBusHelper.RefreshLockAsync", exception: exception);
								}
								catch
								{
									Trace.TraceError("Renewing the {2} lock on {1} '{0}' failed.\r\n{3}", message.MessageId, type, typeName, exception.Message);
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
			}
			catch (ObjectDisposedException) { }
		}
	}
}
