#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace Cqrs.Events
{
	/// <summary>
	/// Publishes an <see cref="IEvent{TAuthenticationToken}"/>
	/// </summary>
	public interface IEventPublisher<TAuthenticationToken>
	{
		void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>;

		void Publish<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<TAuthenticationToken>;
	}
}