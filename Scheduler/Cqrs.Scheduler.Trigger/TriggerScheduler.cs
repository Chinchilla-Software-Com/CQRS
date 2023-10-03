#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Microsoft.Extensions.Configuration;

using Chinchilla.Logging;
using Chinchilla.Logging.Azure.Storage;
using Chinchilla.Logging.Configuration;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Ninject.Configuration;
using Cqrs.Scheduler.Commands;

using ExecutionContext = System.Threading.ExecutionContext;
using Cqrs.Azure.ServiceBus;
using Cqrs.Authentication;
using Chinchilla.StateManagement;
using Cqrs.Bus;

namespace Cqrs.TriggerScheduler
{
	public class TriggerScheduler
	{
		private readonly Microsoft.Extensions.Logging.ILogger<TriggerScheduler> _logger;

		public TriggerScheduler(Microsoft.Extensions.Logging.ILogger<TriggerScheduler> logger)
		{
			_logger = logger;
		}

		[Microsoft.Azure.Functions.Worker.Function("TriggerScheduler")]
#if DEBUG
		// Run every one minute in debug mode
		public void Run([Microsoft.Azure.Functions.Worker.TimerTrigger("0 */1  * * * *")] Microsoft.Azure.Functions.Worker.TimerInfo myTimer, Microsoft.Extensions.Logging.ILogger _logger, ExecutionContext context)
#else
		// Run every 15 minutes in release mode
		public void Run([Microsoft.Azure.Functions.Worker.TimerTrigger("0 1/15 * * * *")] Microsoft.Azure.Functions.Worker.TimerInfo myTimer, Microsoft.Extensions.Logging.ILogger _logger, ExecutionContext context)
#endif
		{
			if (CommandPublisher == null)
				PreapreOnce(context);

			CorrelationIdHelper.SetCorrelationId(Guid.NewGuid());
			CommandPublisher.Publish(new PublishTimeZonesCommand());
			Console.WriteLine($"Published.");
			Logger.LogInfo($"Published.");
		}

		static ICommandPublisher<Guid> CommandPublisher { get; set; }

		static ICorrelationIdHelper CorrelationIdHelper { get; set; }

		static ILogger Logger { get; set; }

		static void PreapreOnce(ExecutionContext context)
		{
			Console.WriteLine("IMPORTANT: Make sure you have read the ReadMeFirst.txt file");

			/*
			// Configure Table storage for logging... see settings file to set connection string
			((NinjectDependencyResolver)DependencyResolver.Current).Kernel.Unbind(typeof(ILogger));
			var logger = new TableStorageLogger(DependencyResolver.Current.Resolve<ILoggerSettings>(), DependencyResolver.Current.Resolve<ICorrelationIdHelper>(), DependencyResolver.Current.Resolve<ITelemetryHelper>());
			((NinjectDependencyResolver)DependencyResolver.Current).Kernel.Bind<ILogger>().ToConstant(logger);
			*/

			// CommandPublisher = DependencyResolver.Current.Resolve<ICommandPublisher<Guid>>();
			CorrelationIdHelper = DependencyResolver.Current.Resolve<ICorrelationIdHelper>();
			Logger = DependencyResolver.Current.Resolve<ILogger>();

			var configurationManager = DependencyResolver.Current.Resolve<IConfigurationManager>();
			var contextItemCollectionFactory = DependencyResolver.Current.Resolve<IContextItemCollectionFactory>();
			var authenticationTokenHelper = new DefaultAuthenticationTokenHelper(contextItemCollectionFactory);
			var messageSerialiser = new MessageSerialiser<Guid>();
			var busHelper = new BusHelper(configurationManager, contextItemCollectionFactory);
			var hashAlgorithmFactory = new BuiltInHashAlgorithmFactory();
			CommandPublisher = new AzureCommandBusPublisher<Guid>
			(
				configurationManager,
				messageSerialiser,
				authenticationTokenHelper,
				CorrelationIdHelper,
				Logger,
				new AzureBusHelper<Guid>(authenticationTokenHelper, CorrelationIdHelper, Logger, messageSerialiser, busHelper, hashAlgorithmFactory, configurationManager, DependencyResolver.Current),
				busHelper,
				hashAlgorithmFactory
			);
		}
	}
}