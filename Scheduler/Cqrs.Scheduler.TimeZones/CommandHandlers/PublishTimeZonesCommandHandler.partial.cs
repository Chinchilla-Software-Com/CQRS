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

namespace Cqrs.Scheduler.CommandHandlers
{
	public partial class PublishTimeZonesCommandHandler : ICommandHandler<Guid, PublishTimeZonesCommand>
	{
		protected ILogger Logger { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		protected IEventPublisher<Guid> EventPublisher { get; private set; }

		protected ITelemetryHelper TelemetryHelper { get; private set; }

		public PublishTimeZonesCommandHandler(ILogger logger, ITelemetryHelper telemetryHelper, IConfigurationManager configurationManager, IEventPublisher<Guid> eventPublisher)
		{
			Logger = logger;
			TelemetryHelper = telemetryHelper;
			ConfigurationManager = configurationManager;
			EventPublisher = eventPublisher;
		}

		public void Handle(PublishTimeZonesCommand message)
		{
			DateTime actualDateTime = DateTime.UtcNow;

			DateTimeOffset processDate = GetNowToTheNearestPrevious15Minutes();

			Logger.LogDebug(string.Format("The calculated time is: '{0}' and the actual time is: '{1}'", processDate, actualDateTime));
			// We Console.WriteLine as this is a WebJob and that will send output to the diagnostic Azure WebJob portal.
			Console.WriteLine("The calculated time is: '{0}' and the actual time is: '{1}'", processDate, actualDateTime);

			FindAndPublishTimeZones(null, processDate);
			FindAndPublishTimeZones(0, processDate);
			FindAndPublishTimeZones(15, processDate);
			FindAndPublishTimeZones(30, processDate);
			FindAndPublishTimeZones(45, processDate);
		}
	}
}