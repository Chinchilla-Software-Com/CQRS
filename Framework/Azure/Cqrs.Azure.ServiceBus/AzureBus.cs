#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Chinchilla.Logging;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Infrastructure;

#if NETSTANDARD2_0 || NET48_OR_GREATER
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using ManagementClient = Azure.Messaging.ServiceBus.Administration.ServiceBusAdministrationClient;
#else
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling;
using RetryPolicy = Microsoft.Practices.TransientFaultHandling.RetryPolicy;
using IMessageReceiver = Microsoft.ServiceBus.Messaging.SubscriptionClient;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// An Azure Bus such as a Service Bus or Event Hub.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <remarks>
	/// https://markheath.net/post/migrating-to-new-servicebus-sdk
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions#receive-messages-from-the-subscription
	/// https://stackoverflow.com/questions/47427361/azure-service-bus-read-messages-sent-by-net-core-2-with-brokeredmessage-getbo
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
	/// </remarks>
	public abstract class AzureBus<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or sets the connection string to the bus.
		/// </summary>
		protected string ConnectionString { get; set; }

		/// <summary>
		/// Gets or sets the RBAC connection settings for the bus.
		/// </summary>
		protected AzureBusRbacSettings RbacConnectionSettings{ get; set; }

		/// <summary>
		/// Gets or sets the <see cref="IMessageSerialiser{TAuthenticationToken}"/>.
		/// </summary>
		protected IMessageSerialiser<TAuthenticationToken> MessageSerialiser { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IEvent{TAuthenticationToken}">events</see> to wait for before responding to the caller
		/// keyed by the <see cref="ICommand{TAuthenticationToken}.Id"/>
		/// </summary>
		protected IDictionary<Guid, IList<IEvent<TAuthenticationToken>>> EventWaits { get; private set; }

		/// <summary>
		/// The default number of receivers to start and run.
		/// </summary>
		protected const int DefaultNumberOfReceiversCount = 1;

		/// <summary>
		/// The number of receivers to start and run.
		/// </summary>
		protected int NumberOfReceiversCount { get; set; }

		/// <summary>
		/// The default number for <see cref="MaximumConcurrentReceiverProcessesCount"/>.
		/// </summary>
		protected const int DefaultMaximumConcurrentReceiverProcessesCount = 1;

#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Used by .NET Framework, but not .Net Core
		/// </summary>
#else
		/// <summary>
		/// The <see cref="OnMessageOptions.MaxConcurrentCalls"/> value.
		/// </summary>
#endif
		protected int MaximumConcurrentReceiverProcessesCount { get; set; }

		/// <summary>
		/// Indicates if the <typeparamref name="TAuthenticationToken"/> is a <see cref="Guid"/>.
		/// </summary>
		protected bool AuthenticationTokenIsGuid { get; private set; }

		/// <summary>
		/// Indicates if the <typeparamref name="TAuthenticationToken"/> is an <see cref="int"/>.
		/// </summary>
		protected bool AuthenticationTokenIsInt { get; private set; }

		/// <summary>
		/// Indicates if the <typeparamref name="TAuthenticationToken"/> is a <see cref="string"/>.
		/// </summary>
		protected bool AuthenticationTokenIsString { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AzureBus{TAuthenticationToken}"/>
		/// </summary>
		protected AzureBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, bool isAPublisher)
		{
			AuthenticationTokenIsGuid = typeof(TAuthenticationToken) == typeof(Guid);
			AuthenticationTokenIsInt = typeof(TAuthenticationToken) == typeof(int);
			AuthenticationTokenIsString = typeof(TAuthenticationToken) == typeof(string);

			EventWaits = new ConcurrentDictionary<Guid, IList<IEvent<TAuthenticationToken>>>();

			MessageSerialiser = messageSerialiser;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CorrelationIdHelper = correlationIdHelper;
			Logger = logger;
			ConfigurationManager = configurationManager;

#if NETSTANDARD2_0 || NET48_OR_GREATER
			UpdateSettingsAsync().Wait();
#else
			UpdateSettings();
#endif
			if (isAPublisher)
#if NETSTANDARD2_0 || NET48_OR_GREATER
				InstantiatePublishingAsync().Wait();
#else
				InstantiatePublishing();
#endif
		}

#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Sets <see cref="ConnectionString"/> from 
		/// <see cref = "GetConnectionStringAsync" />.
		/// </summary>
#else
		/// <summary>
		/// Sets <see cref="ConnectionString"/> from 
		/// <see cref="GetConnectionString"/>.
		/// </summary>
#endif
		protected virtual
#if NETSTANDARD2_0 || NET48_OR_GREATER
			async Task SetConnectionStringsAsync
#else
			void SetConnectionStrings
#endif
			()
		{
			ConnectionString =
#if NETSTANDARD2_0 || NET48_OR_GREATER
				await GetConnectionStringAsync
#else
				GetConnectionString
#endif
				();
			Logger.LogSensitive(string.Format("Connection string settings set to {0}.", ConnectionString));
		}

#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Sets <see cref="RbacConnectionSettings"/> from 
		/// <see cref = "GetRbacConnectionSettingsAsync" />.
		/// </summary>
#else
		/// <summary>
		/// Sets <see cref="RbacConnectionSettings"/> from 
		/// <see cref="GetRbacConnectionSettings"/>.
		/// </summary>
#endif
		protected virtual
#if NETSTANDARD2_0 || NET48_OR_GREATER
			async Task SetRbacConnectionSettingsAsync
#else
			void SetRbacConnectionSettings
#endif
			()
		{
			RbacConnectionSettings =
#if NETSTANDARD2_0 || NET48_OR_GREATER
				await GetRbacConnectionSettingsAsync
#else
				GetRbacConnectionSettings
#endif
				();
			Logger.LogSensitive(string.Format("Connection RBAC settings set to {0}.", RbacConnectionSettings));
		}

		/// <summary>
		/// Sets <see cref="NumberOfReceiversCount"/> from <see cref="GetCurrentNumberOfReceiversCount"/>.
		/// </summary>
		protected virtual void SetNumberOfReceiversCount()
		{
			NumberOfReceiversCount = GetCurrentNumberOfReceiversCount();
			Logger.LogDebug(string.Format("Number of receivers settings set to {0}.", NumberOfReceiversCount));
		}

		/// <summary>
		/// Sets <see cref="MaximumConcurrentReceiverProcessesCount"/> from <see cref="GetCurrentMaximumConcurrentReceiverProcessesCount"/>.
		/// </summary>
		protected virtual void SetMaximumConcurrentReceiverProcessesCount()
		{
			MaximumConcurrentReceiverProcessesCount = GetCurrentMaximumConcurrentReceiverProcessesCount();
			Logger.LogDebug(string.Format("Number of receivers settings set to {0}.", MaximumConcurrentReceiverProcessesCount));
		}

		/// <summary>
		/// Gets the connection string for the bus.
		/// </summary>
		protected abstract
#if NETSTANDARD2_0 || NET48_OR_GREATER
		Task<string> GetConnectionStringAsync
#else
		string GetConnectionString
#endif
			();

		/// <summary>
		/// Gets the RBAC connection settings for the bus from <see cref="AzureBus{TAuthenticationToken}.ConfigurationManager"/>
		/// </summary>
		protected abstract
#if NETSTANDARD2_0 || NET48_OR_GREATER
		Task<AzureBusRbacSettings> GetRbacConnectionSettingsAsync
#else
		AzureBusRbacSettings GetRbacConnectionSettings
#endif
			();

		/// <summary>
		/// Returns <see cref="DefaultNumberOfReceiversCount"/>.
		/// </summary>
		/// <returns><see cref="DefaultNumberOfReceiversCount"/>.</returns>
		protected virtual int GetCurrentNumberOfReceiversCount()
		{
			return DefaultNumberOfReceiversCount;
		}

		/// <summary>
		/// Returns <see cref="DefaultMaximumConcurrentReceiverProcessesCount"/>.
		/// </summary>
		/// <returns><see cref="DefaultMaximumConcurrentReceiverProcessesCount"/>.</returns>
		protected virtual int GetCurrentMaximumConcurrentReceiverProcessesCount()
		{
			return DefaultMaximumConcurrentReceiverProcessesCount;
		}

		/// <summary>
		/// Instantiate publishing on this bus.
		/// </summary>
		protected abstract
#if NETSTANDARD2_0 || NET48_OR_GREATER
		Task InstantiatePublishingAsync
#else
		void InstantiatePublishing
#endif
			();

		/// <summary>
		/// Instantiate receiving on this bus.
		/// </summary>
		protected abstract
#if NETSTANDARD2_0 || NET48_OR_GREATER
		Task InstantiateReceivingAsync
#else
		void InstantiateReceiving
#endif
			();

#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Creates a new instance of <see cref="ManagementClient"/> with the <see cref="ConnectionString"/>.
		/// </summary>
		protected virtual async Task<ManagementClient> GetManagerAsync()
		{

			string connectionString = ConnectionString;
			AzureBusRbacSettings rbacSettings = RbacConnectionSettings;

			ManagementClient manager;
			if (!string.IsNullOrWhiteSpace(connectionString))
				manager = new ManagementClient(ConnectionString);
			else
			{
				var credentials = new ClientSecretCredential(rbacSettings.TenantId, rbacSettings.ApplicationId, rbacSettings.ClientKey);
				manager = new ManagementClient(rbacSettings.Endpoint, credentials);
			}
			return await Task.FromResult(manager);
		}
#else
		/// <summary>
		/// Creates a new instance of <see cref="NamespaceManager"/> with the <see cref="ConnectionString"/>.
		/// </summary>
		protected virtual NamespaceManager GetManager()
		{
			NamespaceManager manager = NamespaceManager.CreateFromConnectionString(ConnectionString);
			return manager;
		}
#endif

#if NETSTANDARD2_0 || NET48_OR_GREATER
#else
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
					var message = $"Retrying action - Count:{args.CurrentRetryCount}, Delay:{args.Delay}";
					Logger.LogWarning(message, "AzureServiceBusRetryPolicy", args.LastException);
				};
				return retryPolicy;
			}
		}
#endif

#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Starts a new <see cref="Task"/> that periodically calls <see cref="ValidateSettingsHaveChangedAsync"/>
		/// and if there is a change, calls <see cref="TriggerSettingsCheckingAsync"/>.
		/// </summary>
#else
		/// <summary>
		/// Starts a new <see cref="Task"/> that periodically calls <see cref="ValidateSettingsHaveChanged"/>
		/// and if there is a change, calls <see cref="TriggerSettingsChecking"/>.
		/// </summary>
#endif
		protected virtual void StartSettingsChecking
			()
		{
			// this is always in a separate task as it runs in the background
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#if NETSTANDARD2_0 || NET48_OR_GREATER
			Task.Factory.StartNewSafelyAsync(async () =>
#else
			Task.Factory.StartNewSafely(() =>
#endif
			{
				SpinWait.SpinUntil(
#if NETSTANDARD2_0 || NET48_OR_GREATER
					() => { return ValidateSettingsHaveChangedAsync().Result; }
#else
					ValidateSettingsHaveChanged
#endif
					, sleepInMilliseconds: 1000);

				Logger.LogInfo("Connecting string settings for the Azure Service Bus changed and will now refresh.");

				// Update the connection string and trigger a restart;
				if (
#if NETSTANDARD2_0 || NET48_OR_GREATER
					await ValidateSettingsHaveChangedAsync
#else
					ValidateSettingsHaveChanged
#endif

					())
#if NETSTANDARD2_0 || NET48_OR_GREATER
					await TriggerSettingsCheckingAsync
#else
					TriggerSettingsChecking
#endif
					();
			});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		/// <summary>
		/// Checks if the settings for
		/// <see cref="ConnectionString"/>, <see cref="NumberOfReceiversCount"/>
		/// or <see cref="MaximumConcurrentReceiverProcessesCount"/> have changed.
		/// </summary>
		/// <returns></returns>
		protected virtual
#if NETSTANDARD2_0 || NET48_OR_GREATER
		async Task<bool> ValidateSettingsHaveChangedAsync
#else
		bool ValidateSettingsHaveChanged
#endif
			()
		{
			return ConnectionString !=
#if NETSTANDARD2_0 || NET48_OR_GREATER
				await GetConnectionStringAsync
#else
				GetConnectionString
#endif
				()
				||
			RbacConnectionSettings.ToString() !=
#if NETSTANDARD2_0 || NET48_OR_GREATER
				(await GetRbacConnectionSettingsAsync())
#else
				GetRbacConnectionSettings()
#endif
					.ToString()
				||
			NumberOfReceiversCount != GetCurrentNumberOfReceiversCount()
				||
			MaximumConcurrentReceiverProcessesCount != GetCurrentMaximumConcurrentReceiverProcessesCount();
		}

#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Calls 
		/// <see cref="SetConnectionStringsAsync"/>
		/// <see cref="SetRbacConnectionSettingsAsync"/>
		/// <see cref="SetNumberOfReceiversCount"/> and 
		/// <see cref="SetMaximumConcurrentReceiverProcessesCount"/>
		/// </summary>
#else
		/// <summary>
		/// Calls 
		/// <see cref="SetConnectionStrings"/>
		/// <see cref="SetRbacConnectionSettings"/>
		/// <see cref="SetNumberOfReceiversCount"/> and 
		/// <see cref="SetMaximumConcurrentReceiverProcessesCount"/>
		/// </summary>
#endif
		protected virtual
#if NETSTANDARD2_0 || NET48_OR_GREATER
			async Task UpdateSettingsAsync
#else
			void UpdateSettings
#endif
			()
		{
#if NETSTANDARD2_0 || NET48_OR_GREATER
			await SetConnectionStringsAsync();
			await SetRbacConnectionSettingsAsync();
#else
			SetConnectionStrings();
			SetRbacConnectionSettings();
#endif
			SetNumberOfReceiversCount();
			SetMaximumConcurrentReceiverProcessesCount();
		}

		/// <summary>
		/// Change the settings used by this bus.
		/// </summary>
		protected abstract
#if NETSTANDARD2_0 || NET48_OR_GREATER
			Task TriggerSettingsCheckingAsync
#else
			void TriggerSettingsChecking
#endif
			();

#if NETSTANDARD2_0 || NET48_OR_GREATER
		/// <summary>
		/// Sets the handler on <see cref="ServiceBusProcessor.ProcessMessageAsync"/>.
		/// </summary>
#else
		/// <summary>
		/// Sets the handler on <see cref="IMessageReceiver.OnMessage(System.Action{Microsoft.ServiceBus.Messaging.BrokeredMessage})"/>.
		/// </summary>
#endif
		protected abstract void ApplyReceiverMessageHandler();
	}
}