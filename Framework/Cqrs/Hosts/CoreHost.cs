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
using System.Net;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Infrastructure;
using Cqrs.Bus;

namespace Cqrs.Hosts
{
	/// <summary>
	/// Configure and start command and event handlers in a host
	/// </summary>
	public abstract class CoreHost<TAuthenticationToken>
	{
		/// <summary>
		/// The <see cref="IConfigurationManager"/> that can be use before the <see cref="DependencyResolver.Current"/> is set.
		/// </summary>
		protected abstract IConfigurationManager ConfigurationManager { get; }

		/// <summary>
		/// Each <see cref="Type"/> will be traced back to it's assembly, and that assembly will be scanned for other handlers to auto register.
		/// </summary>
		protected Type[] HandlerTypes { get; set; }

		/// <summary>
		/// The <see cref="IEventReceiver"/> that will be configured to receive <see cref="IEvent{TAuthenticationToken}">events</see>.
		/// </summary>
		protected IEventReceiver<TAuthenticationToken> EventBus { get; private set; }

		/// <summary>
		/// The <see cref="ICommandReceiver"/> that will be configured to receive <see cref="ICommand{TAuthenticationToken}">commands</see>.
		/// </summary>
		protected ICommandReceiver<TAuthenticationToken> CommandBus { get; private set; }

		/// <summary>
		/// The hosts telemetry name if telemetry is configured
		/// </summary>
		protected string TelemetryName { get; set; }

		/// <summary>
		/// The <see cref="ICorrelationIdHelper"/> that will be used when starting and stopping the host.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// The <see cref="ILogger"/> that will be used when starting and stopping the host.
		/// </summary>
		protected ILogger Logger { get; private set; }

		// ReSharper disable DoNotCallOverridableMethodsInConstructor
		/// <summary>
		/// Instantiate a new instance of a <see cref="CoreHost{TAuthenticationToken}"/>
		/// </summary>
		protected CoreHost()
		{
			HandlerTypes = new Type[]{};
			bool pauseAndWait;
			long waitCounter = 0;
			long maximumCount;
			if (!long.TryParse(ConfigurationManager.GetSetting("Cqrs.Hosts.PauseAndWaitMaximumCount"), out maximumCount))
				maximumCount = 300;
			SpinWait.SpinUntil
			(
				() =>
				{
					if (waitCounter++ > maximumCount)
						return true;
					Console.WriteLine("Pause and wait counter is at {0:N0}", waitCounter);
					return !bool.TryParse(ConfigurationManager.GetSetting("Cqrs.Hosts.PauseAndWait"), out pauseAndWait) || !pauseAndWait;
				},
				(short)1000
			);

			TelemetryName = ConfigurationManager.GetSetting("Cqrs.Hosts.AppName") ?? AppDomain.CurrentDomain.FriendlyName;
		}
		// ReSharper restore DoNotCallOverridableMethodsInConstructor

		/// <summary>
		/// When overridden, allows you to configure Telemetry
		/// </summary>
		protected virtual void ConfigureTelemetry()
		{
		}

		/// <summary>
		/// Calls <see cref="Prepare"/>, <paramref name="handlerRegistation"/> and then <see cref="Start"/>
		/// </summary>
		protected virtual void Run(Action handlerRegistation = null)
		{
			Prepare();
			if (handlerRegistation != null)
				handlerRegistation();
			Start();
		}

		/// <summary>
		/// Sets the <see cref="ServicePointManager.SecurityProtocol"/> to <see cref="SecurityProtocolType.Tls"/>.
		/// You might want to override this to .net 4.5 and above to SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls
		/// </summary>
		protected virtual void PrepareSecurityProtocol()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
		}

		/// <summary>
		/// Prepare the host before registering handlers and starting the host.
		/// </summary>
		protected virtual void Prepare()
		{
			PrepareSecurityProtocol();
			// https://alexandrebrisebois.wordpress.com/2013/03/24/why-are-webrequests-throttled-i-want-more-throughput/
			ServicePointManager.UseNagleAlgorithm = false;
			ServicePointManager.DefaultConnectionLimit = 1000;

			new StartUp(ConfigureDefaultDependencyResolver).Initialise();
			EventBus = DependencyResolver.Current.Resolve<IEventReceiver<TAuthenticationToken>>();
			CommandBus = DependencyResolver.Current.Resolve<ICommandReceiver<TAuthenticationToken>>();
			Guid correlationId = Guid.NewGuid();
			CorrelationIdHelper = DependencyResolver.Current.Resolve<ICorrelationIdHelper>();
			CorrelationIdHelper.SetCorrelationId(correlationId);

			Logger = DependencyResolver.Current.Resolve<ILogger>();
		}

		/// <summary>
		/// Configure the <see cref="DependencyResolver"/>.
		/// </summary>
		protected abstract void ConfigureDefaultDependencyResolver();

		/// <summary>
		/// Start the host post preparing and registering handlers.
		/// </summary>
		protected virtual void Start()
		{
			StartBusRegistrar();

			var configurationManager = DependencyResolver.Current.Resolve<IConfigurationManager>();
			bool setting;
			if (!configurationManager.TryGetSetting("Cqrs.Hosts.EnableEventReceiving", out setting))
				setting = true;
			if (setting)
				EventBus.Start();
			if (!configurationManager.TryGetSetting("Cqrs.Hosts.EnableCommandReceiving", out setting))
				setting = true;
			if (setting)
				CommandBus.Start();
		}

		/// <summary>
		/// Register an event handler that will listen and respond to events.
		/// </summary>
		/// <param name="eventHandler">The event handler to call</param>
		/// <param name="holdMessageLock">If false, this will spin off another thread. This is a bad performance impact. Strongly suggest you use lock renewing instead... which is configuration based... so even better.</param>
		protected virtual void ManuallyRegisterEventHandler<TMessage>(Action<TMessage> eventHandler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			var eventHandlerRegistrar = DependencyResolver.Current.Resolve<IEventHandlerRegistrar>();
			eventHandlerRegistrar.RegisterHandler(eventHandler, holdMessageLock);
		}

		/// <summary>
		/// Register an command handler that will listen and respond to commands.
		/// </summary>
		/// <param name="commandHandler">The command handler to call</param>
		/// <param name="holdMessageLock">If false, this will spin off another thread. This is a bad performance impact. Strongly suggest you use lock renewing instead... which is configuration based... so even better.</param>
		protected virtual void ManuallyRegisterCommandHandler<TMessage>(Action<TMessage> commandHandler, bool holdMessageLock = true)
			where TMessage : IMessage
		{
			var eventHandlerRegistrar = DependencyResolver.Current.Resolve<IEventHandlerRegistrar>();
			eventHandlerRegistrar.RegisterHandler(commandHandler, holdMessageLock);
		}

		/// <summary>
		/// Start the <see cref="BusRegistrar"/> by calling <see cref="BusRegistrar.Register(System.Type[])"/> passing <see cref="HandlerTypes"/>
		/// </summary>
		protected virtual void StartBusRegistrar()
		{
			var registrar = new BusRegistrar(DependencyResolver.Current);
			registrar.Register(HandlerTypes);
		}
	}
}