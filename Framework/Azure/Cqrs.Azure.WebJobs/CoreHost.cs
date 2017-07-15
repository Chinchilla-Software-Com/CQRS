#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Events;
using Cqrs.Messages;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure;
using System.Net;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Infrastructure;
using Cqrs.Bus;

namespace Cqrs.Azure.WebJobs
{
	/// <summary>
	/// Configure and start command and event handlers in an Azure WebJob
	/// </summary>
	public abstract class CoreHost<TAuthenticationToken>
	{
		protected CoreHost()
		{
			HandlerTypes = new Type[]{};
			bool pauseAndWait;
			long waitCounter = 0;
			long maximumCount;
			// I set this to true ... just because.
			if (!long.TryParse(CloudConfigurationManager.GetSetting("Cqrs.Azure.WebJobs.PauseAndWaitMaximumCount", true), out maximumCount))
				maximumCount = 300;
			SpinWait.SpinUntil
			(
				() =>
				{
					if (waitCounter++ > maximumCount)
						return true;
					Console.WriteLine("Pause and wait counter is at {0:N0}", waitCounter);
					return !bool.TryParse(CloudConfigurationManager.GetSetting("Cqrs.Azure.WebJobs.PauseAndWait", false), out pauseAndWait) || !pauseAndWait;
				},
				(short)1000
			);

			// I set this to true ... just because.
			TelemetryName = CloudConfigurationManager.GetSetting("Cqrs.Azure.WebJobs.AppName", true) ?? AppDomain.CurrentDomain.FriendlyName;

			TelemetryConfiguration.Active.InstrumentationKey = CloudConfigurationManager.GetSetting("Cqrs.Azure.WebJobs.ApplicationInsightsInstrumentationKey", true);
			bool enabledApplicationInsightsDeveloperMode;
			if (!bool.TryParse(CloudConfigurationManager.GetSetting("Cqrs.Azure.WebJobs.EnabledApplicationInsightsDeveloperMode", true), out enabledApplicationInsightsDeveloperMode))
				enabledApplicationInsightsDeveloperMode = false;
			TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = enabledApplicationInsightsDeveloperMode;

			TelemetryClient = new TelemetryClient();
			TelemetryClient.InstrumentationKey = TelemetryConfiguration.Active.InstrumentationKey;
			TelemetryClient.TrackEvent(string.Format("{0}/Instantiating", TelemetryName));
			TelemetryClient.Flush();
		}

		/// <summary>
		/// Each <see cref="Type"/> will be traced back to it's assembly, and that assembly will be scanned for other handlers to auto register.
		/// </summary>
		protected Type[] HandlerTypes { get; set; }

		protected IEventReceiver<TAuthenticationToken> EventBus { get; private set; }

		protected ICommandReceiver<TAuthenticationToken> CommandBus { get; private set; }

		public TelemetryClient TelemetryClient { get; private set; }

		protected string TelemetryName { get; set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected ILogger Logger { get; private set; }

		/// <summary>
		/// This actually does all the work... Just sit back and relax... but also stay in memory and don't shutdown.
		/// </summary>
		protected virtual void Run(Action eventHandlerRegistation = null)
		{
			Prepare();
			if (eventHandlerRegistation != null)
				eventHandlerRegistation();
			Start();
			TelemetryClient.TrackEvent(string.Format("{0}/Ran", TelemetryName));
			TelemetryClient.Flush();
		}

		protected virtual void Prepare()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			// https://alexandrebrisebois.wordpress.com/2013/03/24/why-are-webrequests-throttled-i-want-more-throughput/
			ServicePointManager.UseNagleAlgorithm = false;
			ServicePointManager.DefaultConnectionLimit = 1000;

			new StartUp(ConfigureDefaultDependencyResolver).Initialise();
			EventBus = DependencyResolver.Current.Resolve<IEventReceiver<TAuthenticationToken>>();
			CommandBus = DependencyResolver.Current.Resolve<ICommandReceiver<TAuthenticationToken>>();
			Guid correlationId = Guid.NewGuid();
			CorrelationIdHelper = DependencyResolver.Current.Resolve<ICorrelationIdHelper>();
			CorrelationIdHelper.SetCorrelationId(correlationId);

			TelemetryClient.TrackEvent(string.Format("{0}/Prepared", TelemetryName));

			Logger = DependencyResolver.Current.Resolve<ILogger>();
		}

		protected abstract void ConfigureDefaultDependencyResolver();

		protected virtual void Start()
		{
			StartBusRegistrar();

			var configurationManager = DependencyResolver.Current.Resolve<IConfigurationManager>();
			bool setting;
			if (!configurationManager.TryGetSetting("Cqrs.Azure.WebJobs.EnableEventReceiving", out setting))
				setting = true;
			if (setting)
				EventBus.Start();
			if (!configurationManager.TryGetSetting("Cqrs.Azure.WebJobs.EnableCommandReceiving", out setting))
				setting = true;
			if (setting)
				CommandBus.Start();
			TelemetryClient.TrackEvent(string.Format("{0}/Started", TelemetryName));
		}

		/// <summary>
		/// Register an event handler that will listen and respond to events.
		/// </summary>
		/// <param name="eventHandler">The event handler to call</param>
		/// <param name="holdMessageLock">If false, this will spin off another thread. This is a bad performance impact. Strongly suggest you use lock renewing instead... which is configuration based... so even better.</param>
		protected virtual void ManuallyRegisterEventHandler<TMessage>(Action<TMessage> eventHandler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			var eventBus = DependencyResolver.Current.Resolve<IEventHandlerRegistrar>();
			eventBus.RegisterHandler(eventHandler, holdMessageLock);
		}

		/// <summary>
		/// Register an command handler that will listen and respond to commands.
		/// </summary>
		/// <param name="commandHandler">The command handler to call</param>
		/// <param name="holdMessageLock">If false, this will spin off another thread. This is a bad performance impact. Strongly suggest you use lock renewing instead... which is configuration based... so even better.</param>
		protected virtual void ManuallyRegisterCommandHandler<TMessage>(Action<TMessage> commandHandler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			var eventBus = DependencyResolver.Current.Resolve<IEventHandlerRegistrar>();
			eventBus.RegisterHandler(commandHandler, holdMessageLock);
		}

		protected virtual void StartBusRegistrar()
		{
			var registrar = new BusRegistrar(DependencyResolver.Current);
			registrar.Register(HandlerTypes);
		}
	}
}