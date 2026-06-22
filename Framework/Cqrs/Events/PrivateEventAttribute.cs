using System;

namespace Cqrs.Events
{
	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> that should be sent via the private <see cref="IEventPublisher{TAuthenticationToken}"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	[Serializable]
	public class PrivateEventAttribute
		: Attribute
	{
	}
}