using System;
using System.Runtime.Serialization;

namespace Cqrs.Scheduler.Events
{
	/// <summary>
	/// A <see cref="TimeZoneEvent"/> indicating a <see cref="TimeZoneInfo">time-zone</see> was 45 minutes past the hour.
	/// </summary>
	[Serializable]
	[DataContract]
	public class ItsQuarterToTheHourEvent : TimeZoneEvent
	{
	}
}