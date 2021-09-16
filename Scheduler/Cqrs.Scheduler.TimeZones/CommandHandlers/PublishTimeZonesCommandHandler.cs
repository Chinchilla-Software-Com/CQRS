#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cqrs.Commands;
using Cqrs.Scheduler.Commands;
using Cqrs.Scheduler.Events;

namespace Cqrs.Scheduler.CommandHandlers
{
	public partial class PublishTimeZonesCommandHandler : ICommandHandler<Guid, PublishTimeZonesCommand>
	{
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
		/// Find all <see cref="TimeZoneInfo">time-zones</see> where the <see cref="DateTime.Minute"/> of <paramref name="processDate"/> equals <paramref name="minutes"/>.
		/// If <paramref name="minutes"/> is null then get all <see cref="TimeZoneInfo">time-zones</see> that are at midnight.
		/// Raises a notification for each found <see cref="TimeZoneInfo">time-zone</see>.
		/// </summary>
		protected virtual void FindAndPublishTimeZones(short? minutes, DateTimeOffset processDate)
		{
			string appSettingName;
			bool defaultRunSetting;
			Type eventType;
			switch (minutes)
			{
				case null:
					appSettingName = "Midnight";
					defaultRunSetting = true;
					eventType = typeof(ItsMidnightInEvent);
					break;
				case 0:
					appSettingName = "OnTheHour";
					defaultRunSetting = false;
					eventType = typeof(ItsOnTheHourEvent);
					break;
				case 15:
					appSettingName = "QuarterPastTheHour";
					defaultRunSetting = false;
					eventType = typeof(ItsQuarterToTheHourEvent);
					break;
				case 30:
					appSettingName = "HalfPastTheHour";
					defaultRunSetting = false;
					eventType = typeof(ItsHalfPastTheHourEvent);
					break;
				case 45:
					appSettingName = "QuarterToTheHour";
					defaultRunSetting = false;
					eventType = typeof(ItsQuarterPastTheHourEvent);
					break;
				default:
					return;
			}
			bool findTimeZones;
			if (!ConfigurationManager.TryGetSetting(string.Format("Cqrs.Scheduler.Find{0}TimeZones", appSettingName), out findTimeZones))
				findTimeZones = defaultRunSetting;
			if (!findTimeZones)
				return;

			string eventName = eventType.Name;

			Stopwatch stopWatch = Stopwatch.StartNew();
			IEnumerable<(TimeZoneInfo TimeZone, short Hour)> midnightTimezones = GetTimezonesAt(processDate, minutes).ToList();
			foreach ((TimeZoneInfo TimeZone, short Hour) pair in midnightTimezones)
			{
				TimeZoneInfo timeZoneInfo = pair.TimeZone;
				try
				{
					var @event = (TimeZoneEvent)Activator.CreateInstance(eventType);
					@event.TimezoneId = timeZoneInfo.Id;
					@event.ProcessDate = processDate;
					@event.LocalHour = pair.Hour;
					EventPublisher.Publish(@event);
				}
				catch (Exception exception)
				{
					Logger.LogWarning(string.Format("Sending the {0} for time-zone '{1}' Failed.", eventName, timeZoneInfo.Id), exception: exception);
					TelemetryHelper.TrackException(exception);
				}
			}
		}

		/// <summary>
		/// Get all <see cref="TimeZoneInfo">time-zones</see> where the <see cref="DateTime.Minute"/> of <paramref name="processDate"/> equals <paramref name="minutes"/>.
		/// If <paramref name="minutes"/> is null then get all <see cref="TimeZoneInfo">time-zones</see> that are at midnight.
		/// </summary>
		/// <param name="processDate">The <see cref="DateTimeOffset">time</see> to process.</param>
		/// <param name="minutes">Indicates if the number of minutes from the hour. 0, 15, 30 or 45 are valid values.</param>
		/// <returns>An <see cref="ICollection{T}"/> where each <see cref="Tuple{TimeZoneInfo, TMinute}">item</see> is the <see cref="TimeZoneInfo"/> and the number of minutes past the hour, so 45 for 45 minutes past the hour.</returns>
		protected virtual IEnumerable<(TimeZoneInfo, short)> GetTimezonesAt(DateTimeOffset processDate, short? minutes = null)
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

			var timezones = new List<(TimeZoneInfo Timezone, short Hour)>();

			bool onlyProcessUtcTimezone;
			if (!ConfigurationManager.TryGetSetting("Cqrs.Scheduler.OnlyProcessUtcTimezone", out onlyProcessUtcTimezone))
				onlyProcessUtcTimezone = true;

			IEnumerable<TimeZoneInfo> timezonesToProcess;

			if (onlyProcessUtcTimezone)
				timezonesToProcess = new List<TimeZoneInfo> { TimeZoneInfo.Utc };
			else
				timezonesToProcess = TimeZoneInfo.GetSystemTimeZones();

			foreach (TimeZoneInfo systemTimeZone in timezonesToProcess)
			{
				TimeSpan utcOffset = systemTimeZone.GetUtcOffset(processDate);
				Logger.LogDebug(string.Format("Server is operating with '{0}'.", utcOffset));

				DateTimeOffset adjustedProcessDate = processDate.ToOffset(utcOffset);

				DateTimeOffset expectedDateTimeOffset = new DateTimeOffset(adjustedProcessDate.Year, adjustedProcessDate.Month, adjustedProcessDate.Day, 0, 0, 0, utcOffset);
				TimeSpan diff = adjustedProcessDate - expectedDateTimeOffset;

				// If minutes is null and TotalMinutes == 0 it's midnight, otherwise check the number of minutes from the current hour
				if ((minutes == null && Math.Truncate(diff.TotalMinutes).Equals(0)) || (minutes != null && diff.Minutes == minutes))
				{
					timezones.Add((systemTimeZone, (short)diff.Hours));
					Logger.LogDebug(string.Format("Found time-zone '{0}' for {3} '{1:00}:{2:00}'", systemTimeZone, processDate.Hour, processDate.Minute, timeValue));
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
				Logger.LogDebug(string.Format("No time-zones for {2} '{0:00}:{1:00}'", processDate.Hour, processDate.Minute, timeValue));
				// We Console.WriteLine as this is a WebJob and that will send output to the diagnostic Azure WebJob portal.
				Console.WriteLine("No time-zones for {2} '{0:00}:{1:00}'", processDate.Hour, processDate.Minute, timeValue);
			}
			else
			{
				Logger.LogInfo(string.Format("Found '{0}' time-zones for {3} '{1:00}:{2:00}'", timezones.Count, processDate.Hour, processDate.Minute, timeValue));
				// We Console.WriteLine as this is a WebJob and that will send output to the diagnostic Azure WebJob portal.
				Console.WriteLine("Found '{0}' time-zone{4} for {3} '{1:00}:{2:00}'", timezones.Count, processDate.Hour, processDate.Minute, timeValue, timezones.Count == 1 ? string.Empty : "s");
			}

			return timezones;
		}
	}
}
