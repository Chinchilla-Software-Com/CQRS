using System;
using System.Runtime.Serialization;

namespace $safeprojectname$
{
	/// <summary>
	/// A <see cref="TimeZoneEvent"/> indicating a <see cref="TimeZoneInfo">time-zone</see> was 30 minutes past the hour.
	/// </summary>
	[Serializable]
	[DataContract]
	public class ItsHalfPastTheHourEvent : TimeZoneEvent
	{
	}
}