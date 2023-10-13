using System;
using System.Threading;
using System.Threading.Tasks;
using Chinchilla.Logging;
using Chinchilla.StateManagement;
using Cqrs.Authentication;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.DependencyInjection;

namespace Cqrs.Scheduler.Trigger
{
	public class TimedTrigger
	{
		[Microsoft.Azure.Functions.Worker.Function(nameof(TimedTrigger))]
#if DEBUG
		public async Task Run([Microsoft.Azure.Functions.Worker.TimerTrigger("0 */1 * * * *")] Microsoft.Azure.Functions.Worker.TimerInfo myTimer, Microsoft.Extensions.Logging.ILogger log, ExecutionContext context)
#else
		// Run every 15 minutes in release mode
		public async Task Run([Microsoft.Azure.Functions.Worker.TimerTrigger("0 */15 * * * *")] Microsoft.Azure.Functions.Worker.TimerInfo myTimer, Microsoft.Extensions.Logging.ILogger log, ExecutionContext context)
#endif
		{
			if (CommandPublisher == null)
				PreapreOnce(context);

			CorrelationIdHelper.SetCorrelationId(Guid.NewGuid());
			await CommandPublisher.PublishAsync(new Cqrs.Scheduler.Commands.PublishTimeZonesCommand());
			Console.WriteLine($"Published.");
			Logger.LogInfo($"Published.");
		}

		static IAsyncCommandPublisher<Guid> CommandPublisher { get; set; }

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

			var configurationManager = DependencyResolver.Current.Resolve<Cqrs.Configuration.IConfigurationManager>();
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