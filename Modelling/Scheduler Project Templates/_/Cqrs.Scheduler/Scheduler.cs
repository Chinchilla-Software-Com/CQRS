using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration;
using Cqrs.Ninject.Azure.WebJobs;
using $safeprojectname$.Events;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Ninject.Modules;

namespace $safeprojectname$
{
	/// <summary>
	/// A micro-service scheduler that notifies the system on a schedule with 15 minute increments.
	/// </summary>
	public class Scheduler : CqrsNinjectJobHost<Guid, DefaultAuthenticationTokenHelper>
	{
		/// <summary>
		/// Raises notifications to the system advising what time it is in different <see cref="TimeZoneInfo">time-zones</see>.
		/// </summary>
		public virtual void WhatTimeIsIt()
		{
			Guid correlationId = CorrelationIdHelper.GetCorrelationId();
			Stopwatch mainStopWatch = Stopwatch.StartNew();
			var requestTelemetry = new RequestTelemetry
			{
				Name = "Scheduler/WhatTimeIsIt",
				Timestamp = DateTimeOffset.UtcNow,
				ResponseCode = "200",
				Success = true,
				Id = correlationId.ToString("N"),
				Sequence = correlationId.ToString("N")
			};

			try
			{
				var actualDateTime = DateTime.UtcNow;

				DateTimeOffset processDate = GetNowToTheNearestPrevious15Minutes();

				Logger.LogInfo(string.Format("The calculated time is: '{0}' and the actual time is: '{1}'", processDate, actualDateTime));
				// We Console.WriteLine as this is a WebJob and that will send output to the diagnostic Azure WebJob portal.
				Console.WriteLine("The calculated time is: '{0}' and the actual time is: '{1}'", processDate, actualDateTime);

				var eventPublisher = DependencyResolver.Current.Resolve<IEventPublisher<Guid>>();
				FindAndPublishTimeZones(eventPublisher, requestTelemetry, null, processDate);
				FindAndPublishTimeZones(eventPublisher, requestTelemetry, 0, processDate);
				FindAndPublishTimeZones(eventPublisher, requestTelemetry, 15, processDate);
				FindAndPublishTimeZones(eventPublisher, requestTelemetry, 30, processDate);
				FindAndPublishTimeZones(eventPublisher, requestTelemetry, 45, processDate);
			}
			catch
			{
				requestTelemetry.ResponseCode = "500";
				requestTelemetry.Success = false;
				throw;
			}
			finally
			{
				Logger.LogInfo("Done... Shutting down.");

				mainStopWatch.Stop();
				requestTelemetry.Duration = mainStopWatch.Elapsed;
				TelemetryClient.TrackRequest(requestTelemetry);

				// We need to do this, otherwise if the app exits the telemetry data won't be sent
				TelemetryClient.Flush();
			}
		}

		/// <summary>
		/// Find all <see cref="TimeZoneInfo">time-zones</see> where the <see cref="DateTime.Minute"/> of <paramref name="processDate"/> equals <paramref name="minutes"/>.
		/// If <paramref name="minutes"/> is null then get all <see cref="TimeZoneInfo">time-zones</see> that are at midnight.
		/// Raises a notification for each found <see cref="TimeZoneInfo">time-zone</see>.
		/// </summary>
		protected virtual void FindAndPublishTimeZones(IEventPublisher<Guid> eventPublisher, RequestTelemetry requestTelemetry, short? minutes, DateTimeOffset processDate)
		{
			string appSettingName;
			bool defaultRunSetting;
			Type eventType;
			switch (minutes)
			{
				case null:
					appSettingName = "Midnight";
					defaultRunSetting = true;
					eventType = typeof (ItsMidnightInEvent);
					break;
				case 0:
					appSettingName = "OnTheHour";
					defaultRunSetting = false;
					eventType = typeof (ItsOnTheHourEvent);
					break;
				case 15:
					appSettingName = "QuarterPastTheHour";
					defaultRunSetting = false;
					eventType = typeof (ItsQuarterToTheHourEvent);
					break;
				case 30:
					appSettingName = "HalfPastTheHour";
					defaultRunSetting = false;
					eventType = typeof(ItsHalfPastTheHourEvent);
					break;
				case 45:
					appSettingName = "QuarterToTheHour";
					defaultRunSetting = false;
					eventType = typeof (ItsQuarterPastTheHourEvent);
					break;
				default:
					return;
			}
			bool findTimeZones;
			if (!ConfigurationManager.TryGetSetting(string.Format("$safeprojectname$.Find{0}TimeZones", appSettingName), out findTimeZones))
				findTimeZones = defaultRunSetting;
			if (!findTimeZones)
				return;

			string eventName = eventType.Name;

			Stopwatch stopWatch = Stopwatch.StartNew();
			IEnumerable<Tuple<TimeZoneInfo, short>> midnightTimezones = GetTimezonesAt(processDate, minutes).ToList();
			foreach (Tuple<TimeZoneInfo, short> pair in midnightTimezones)
			{
				TimeZoneInfo timeZoneInfo = pair.Item1;
				try
				{
					var @event = (TimeZoneEvent)Activator.CreateInstance(eventType);
					@event.TimezoneId = timeZoneInfo.Id;
					@event.ProcessDate = processDate;
					@event.LocalHour = pair.Item2;
					eventPublisher.Publish(@event);
				}
				catch (Exception exception)
				{
					Logger.LogWarning(string.Format("Sending the {0} for time-zone '{1}' Failed.", eventName, timeZoneInfo.Id), exception: exception);
					TelemetryClient.TrackException(exception);
				}
			}
			stopWatch.Stop();
			requestTelemetry.Metrics.Add(string.Format("Scheduler/{0}", eventName), stopWatch.ElapsedMilliseconds);
		}

		/// <summary>
		/// Gets the current time to the nearest (in the past) 15 minutes.
		/// </summary>
		protected virtual DateTimeOffset GetNowToTheNearestPrevious15Minutes()
		{
			DateTimeOffset processDate = DateTimeOffset.Now;
			processDate = processDate.AddMinutes(0 - processDate.Minute % 15);
			processDate = processDate.AddSeconds(0 - processDate.Second);
			return processDate;
		}

		/// <summary>
		/// Get all <see cref="TimeZoneInfo">time-zones</see> where the <see cref="DateTime.Minute"/> of <paramref name="processDate"/> equals <paramref name="minutes"/>.
		/// If <paramref name="minutes"/> is null then get all <see cref="TimeZoneInfo">time-zones</see> that are at midnight.
		/// </summary>
		/// <param name="processDate">The <see cref="DateTimeOffset">time</see> to process.</param>
		/// <param name="minutes">Indicates if the number of minutes from the hour. 0, 15, 30 or 45 are valid values.</param>
		/// <returns>An <see cref="ICollection{T}"/> where each <see cref="Tuple{TimeZoneInfo, TMinute}">item</see> is the <see cref="TimeZoneInfo"/> and the number of minutes past the hour, so 45 for 45 minutes past the hour.</returns>
		protected virtual IEnumerable<Tuple<TimeZoneInfo, short>> GetTimezonesAt(DateTimeOffset processDate, short? minutes = null)
		{
			string timeValue = "now";
			switch (minutes)
			{
				case null:
					timeValue = "midnight";
					break;
				case 0:
					timeValue = "this hour";
					break;
				case 15:
					timeValue = "this quarter past the hour";
					break;
				case 30:
					timeValue = "this half past the hour";
					break;
				case 45:
					timeValue = "this quarter to the hour";
					break;
			}

			var timezones = new List<Tuple<TimeZoneInfo, short>>();
			foreach (TimeZoneInfo systemTimeZone in TimeZoneInfo.GetSystemTimeZones())
			{
				TimeSpan utcOffset = systemTimeZone.GetUtcOffset(processDate);
				Logger.LogDebug(string.Format("Server is operating with '{0}'.", utcOffset));

				DateTimeOffset adjustedProcessDate = processDate.ToOffset(utcOffset);

				DateTimeOffset expectedDateTimeOffset = new DateTimeOffset(adjustedProcessDate.Year, adjustedProcessDate.Month, adjustedProcessDate.Day, 0, 0, 0, utcOffset);
				TimeSpan diff = adjustedProcessDate - expectedDateTimeOffset;

				// If minutes is null and TotalMinutes == 0 it's midnight, otherwise check the number of minutes from the current hour
				if ((minutes == null && Math.Truncate(diff.TotalMinutes).Equals(0)) || (minutes != null && diff.Minutes == minutes))
				{
					timezones.Add(new Tuple<TimeZoneInfo, short>(systemTimeZone, (short)diff.Hours));
					Logger.LogInfo(string.Format("Found time-zone '{0}' for {3} '{1:00}:{2:00}'", systemTimeZone, processDate.Hour, processDate.Minute, timeValue));
					// We Console.WriteLine as this is a WebJob and that will send output to the diagnostic Azure WebJob portal.
					Console.WriteLine("Found time-zone '{0}' for {3} '{1:00}:{2:00}'", systemTimeZone, processDate.Hour, processDate.Minute, timeValue);
				}
				else
				{
					if (minutes == null && !Math.Truncate(diff.TotalMinutes).Equals(0))
						Logger.LogDebug(string.Format("Time-zone '{0}' is not for {3} '{1:00}:{2:00}' as difference is '{4}'", systemTimeZone, processDate.Hour, processDate.Minute, timeValue, Math.Truncate(diff.TotalMinutes)));
					else if (minutes != null && diff.Minutes != minutes)
						Logger.LogDebug(string.Format("Time-zone '{0}' is not for {3} '{1:00}:{2:00}' as calculated minutes are '{4}' which doesn't equal '{2}'", systemTimeZone, processDate.Hour, processDate.Minute, timeValue, minutes));
					else
						Logger.LogDebug(string.Format("Time-zone '{0}' is not for {3} '{1:00}:{2:00}'.", systemTimeZone, processDate.Hour, processDate.Minute, timeValue));
				}
			}

			if (!timezones.Any())
			{
				Logger.LogInfo(string.Format("No time-zones for {2} '{0:00}:{1:00}'", processDate.Hour, processDate.Minute, timeValue));
				// We Console.WriteLine as this is a WebJob and that will send output to the diagnostic Azure WebJob portal.
				Console.WriteLine("No time-zones for {2} '{0:00}:{1:00}'", processDate.Hour, processDate.Minute, timeValue);
			}
			else
			{
				Logger.LogInfo(string.Format("Found '{0}' time-zones for {3} '{1:00}:{2:00}'", timezones.Count, processDate.Hour, processDate.Minute, timeValue));
				// We Console.WriteLine as this is a WebJob and that will send output to the diagnostic Azure WebJob portal.
				Console.WriteLine("Found '{0}' time-zones for {3} '{1:00}:{2:00}'", timezones.Count, processDate.Hour, processDate.Minute, timeValue);
			}

			return timezones;
		}

		#region Overrides of CoreHost<Guid>

		/// <summary>
		/// Disabled the <see cref="BusRegistrar"/> as we're only sending <see cref="IEvent{TAuthenticationToken}">events</see> and instead trigger some Azure WebJob Stuff.
		/// </summary>
		protected override void StartBusRegistrar()
		{
			// Only one is needed per assembly. If you have events in another assembly, add ONE of those to this array in addition.
			JobHost host;
			bool disableHostControl;
			// I set this to true ... just because.
			if (!bool.TryParse(_configurationManager.GetSetting("Cqrs.Azure.WebJobs.DisableWebJobHostControl"), out disableHostControl))
				disableHostControl = false;
			if (disableHostControl)
			{
				var jobHostConfig = new JobHostConfiguration
				{
					// https://github.com/Azure/azure-webjobs-sdk/issues/741
					DashboardConnectionString = null
				};
				host = new JobHost(jobHostConfig);
			}
			else
				host = new JobHost();
		}

		#endregion

		#region Overrides of CqrsWebHost<Guid,DefaultAuthenticationTokenHelper,WebJobHostModule>

		/// <summary>
		/// Disables configuring the Azure Servicebus as a command bus. Remove this override if you want to send and receive <see cref="ICommand{TAuthenticationToken}">commands</see> from this application.
		/// </summary>
		protected override IEnumerable<INinjectModule> GetCommandBusModules()
		{
			return Enumerable.Empty<INinjectModule>();
		}

		/// <summary>
		/// Configuring the Azure Servicebus as an event bus that only publishes <see cref="IEvent{Guid}">events</see>.
		/// </summary>
		protected override IEnumerable<INinjectModule> GetEventBusModules()
		{
			return new List<INinjectModule>
			{
				new AzureEventBusPublisherModule<Guid>()
			};
		}

		#endregion
	}
}