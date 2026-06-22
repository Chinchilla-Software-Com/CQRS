using System;

namespace Cqrs.Events
{
	/// <summary>
	/// A set of extension method for <see cref="IEvent{TAuthenticationToken}"/>.
	/// </summary>
	public static class EventExtensions
	{
		/// <summary>
		/// The identity of the target object of the provided <paramref name="event"/>.
		/// </summary>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> to locate the identify from.</param>
		/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
		/// <returns><see cref="IEventWithIdentity{TAuthenticationToken}.Rsn"/> or <see cref="IEvent{TAuthenticationToken}.Id"/>.</returns>
		public static Guid GetIdentity<TAuthenticationToken>(this IEvent<TAuthenticationToken> @event)
		{
			var eventWithIdentity = @event as IEventWithIdentity<TAuthenticationToken>;
			Guid rsn = eventWithIdentity == null ? @event.Id : eventWithIdentity.Rsn;
			return rsn;
		}
	}
}