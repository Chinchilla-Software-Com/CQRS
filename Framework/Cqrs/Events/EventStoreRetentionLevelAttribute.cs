using System;

namespace Cqrs.Events
{
	/// <summary>
	/// Provides a mechanism to configure different <see cref="IEventStore{TAuthenticationToken}"/> instances to save the event to.
	/// This is useful if you have events that should be in hot storage with quick loads and events that should be in cold storage and are unlikely to be reloaded and use slower cheaper storage.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class EventStoreRetentionLevelAttribute : Attribute
	{
		/// <summary>
		/// The level of retention required. By specifying a value we look for a matching configured <see cref="IEventStore{TAuthenticationToken}"/> with the same level defined.
		/// </summary>
		public short RetentionLevel { get; set; }
	}
}