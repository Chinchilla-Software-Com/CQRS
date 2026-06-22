#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Configuration;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Scheduler.Commands;
using Chinchilla.Logging;
using System.Threading.Tasks;

namespace Cqrs.Scheduler.CommandHandlers
{
	public partial class PublishTimeZonesCommandHandler : ICommandHandler<Guid, PublishTimeZonesCommand>
	{
		protected ILogger Logger { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected IAsyncEventPublisher<Guid> EventPublisher { get; private set; }

		protected ITelemetryHelper TelemetryHelper { get; private set; }

		public PublishTimeZonesCommandHandler(ILogger logger, ITelemetryHelper telemetryHelper, IConfigurationManager configurationManager, IAsyncEventPublisher<Guid> eventPublisher)
		{
			Logger = logger;
			TelemetryHelper = telemetryHelper;
			ConfigurationManager = configurationManager;
			EventPublisher = eventPublisher;
		}

		public virtual async Task HandleAsync(PublishTimeZonesCommand message)
		{
			DateTime actualDateTime = DateTime.UtcNow;

			DateTimeOffset processDate = GetNowToTheNearestPrevious15Minutes();

			string logMessage = $"The calculated time is: '{processDate}' and the actual time is: '{actualDateTime}'";
			Logger.LogDebug(logMessage);
			// We Console.WriteLine as this is a WebJob and that will send output to the diagnostic Azure WebJob portal.
			Console.WriteLine(logMessage);

			await FindAndPublishTimeZones(null, processDate);
			await FindAndPublishTimeZones(0, processDate);
			await FindAndPublishTimeZones(15, processDate);
			await FindAndPublishTimeZones(30, processDate);
			await FindAndPublishTimeZones(45, processDate);
		}
	}
}