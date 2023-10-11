#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cqrs.Events
{
	/// <summary>
	/// Publishes an <see cref="IEvent{TAuthenticationToken}"/>
	/// </summary>
	public interface IAsyncEventPublisher<TAuthenticationToken>
	{
		/// <summary>
		/// Publishes the provided <paramref name="event"/> on the event bus.
		/// </summary>
		Task PublishAsync<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>;

		/// <summary>
		/// Publishes the provided <paramref name="events"/> on the event bus.
		/// </summary>
		Task PublishAsync<TEvent>(IEnumerable<TEvent> events)
			where TEvent : IEvent<TAuthenticationToken>;
	}
}