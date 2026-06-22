using System;

namespace Cqrs.Events
{
	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that should be sent to all connected people except the person who was authenticated when sending the request via SignalR.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	[Serializable]
	public class NotifyEveryoneExceptCallerEventAttribute
		: Attribute
	{
	}
}