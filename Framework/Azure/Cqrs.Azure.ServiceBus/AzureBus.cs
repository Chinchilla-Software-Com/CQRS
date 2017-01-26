#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Cqrs.Events;
using Cqrs.Infrastructure;
using Microsoft.ServiceBus;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling;
using RetryPolicy = Microsoft.Practices.TransientFaultHandling.RetryPolicy;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureBus<TAuthenticationToken>
	{
		protected string ConnectionString { get; set; }

		protected IMessageSerialiser<TAuthenticationToken> MessageSerialiser { get; private set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected IDictionary<Guid, IList<IEvent<TAuthenticationToken>>> EventWaits { get; private set; }

		protected const int DefaultNumberOfReceiversCount = 1;

		protected int NumberOfReceiversCount { get; set; }

		protected const int DefaultMaximumConcurrentReceiverProcessesCount = 1;

		protected int MaximumConcurrentReceiverProcessesCount { get; set; }

		protected AzureBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, bool isAPublisher)
		{
			EventWaits = new ConcurrentDictionary<Guid, IList<IEvent<TAuthenticationToken>>>();

			MessageSerialiser = messageSerialiser;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
			ConfigurationManager = configurationManager;

			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			UpdateSettings();
			if (isAPublisher)
				InstantiatePublishing();
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		protected virtual void SetConnectionStrings()
		{
			ConnectionString = GetConnectionString();
			Logger.LogSensitive(string.Format("Connection string settings set to {0}.", ConnectionString));
		}

		protected virtual void SetNumberOfReceiversCount()
		{
			NumberOfReceiversCount = GetCurrentNumberOfReceiversCount();
			Logger.LogDebug(string.Format("Number of receivers settings set to {0}.", NumberOfReceiversCount));
		}

		protected virtual void SetMaximumConcurrentReceiverProcessesCount()
		{
			MaximumConcurrentReceiverProcessesCount = GetCurrentMaximumConcurrentReceiverProcessesCount();
			Logger.LogDebug(string.Format("Number of receivers settings set to {0}.", MaximumConcurrentReceiverProcessesCount));
		}

		protected abstract string GetConnectionString();

		protected virtual int GetCurrentNumberOfReceiversCount()
		{
			return DefaultNumberOfReceiversCount;
		}

		protected virtual int GetCurrentMaximumConcurrentReceiverProcessesCount()
		{
			return DefaultMaximumConcurrentReceiverProcessesCount;
		}

		protected abstract void InstantiatePublishing();

		protected abstract void InstantiateReceiving();

		protected virtual NamespaceManager GetNamespaceManager()
		{
			NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
			return namespaceManager;
		}

		/// <summary>
		/// Gets the default retry policy dedicated to handling transient conditions with Windows Azure Service Bus.
		/// </summary>
		protected virtual RetryPolicy AzureServiceBusRetryPolicy
		{
			get
			{
				RetryManager retryManager = EnterpriseLibraryContainer.Current.GetInstance<RetryManager>();
				RetryPolicy retryPolicy = retryManager.GetDefaultAzureServiceBusRetryPolicy();
				retryPolicy.Retrying += (sender, args) =>
				{
					var message = string.Format("Retrying action - Count:{0}, Delay:{1}", args.CurrentRetryCount, args.Delay);
					Logger.LogWarning(message, "AzureServiceBusRetryPolicy", args.LastException);
				};
				return retryPolicy;
			}
		}

		protected virtual void StartSettingsChecking()
		{
			Task.Factory.StartNewSafely(() =>
			{
				SpinWait.SpinUntil(ValidateSettingsHaveChanged, sleepInMilliseconds: 1000);

				Logger.LogInfo("Connecting string settings for the Azure Service Bus changed and will now refresh.");

				// Update the connection string and trigger a restart;
				if (ValidateSettingsHaveChanged())
					TriggerSettingsChecking();
			});
		}

		protected virtual bool ValidateSettingsHaveChanged()
		{
			return ConnectionString != GetConnectionString()
				||
			NumberOfReceiversCount != GetCurrentNumberOfReceiversCount()
				||
			MaximumConcurrentReceiverProcessesCount != GetCurrentMaximumConcurrentReceiverProcessesCount();
		}

		protected virtual void UpdateSettings()
		{
			SetConnectionStrings();
			SetNumberOfReceiversCount();
			SetMaximumConcurrentReceiverProcessesCount();
		}

		protected abstract void TriggerSettingsChecking();

		protected abstract void ApplyReceiverMessageHandler();
	}
}