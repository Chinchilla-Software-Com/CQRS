using System;
using System.Runtime.Serialization;

namespace Cqrs.Scheduler.Events
{
	/// <summary>
	/// A <see cref="TimeZoneEvent"/> indicating a <see cref="TimeZoneInfo">time-zone</see> was at mid-night.
	/// </summary>
	[Serializable]
	[DataContract]
	public class ItsMidnightInEvent : TimeZoneEvent
	{
	}
}