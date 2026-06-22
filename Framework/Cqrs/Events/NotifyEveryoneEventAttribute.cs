using System;

namespace Cqrs.Events
{
	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that should be sent to all connected people via SignalR.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	[Serializable]
	public class NotifyEveryoneEventAttribute
		: Attribute
	{
	}
}