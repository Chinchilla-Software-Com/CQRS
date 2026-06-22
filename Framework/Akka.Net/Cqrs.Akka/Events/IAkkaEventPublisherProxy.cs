#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Cqrs.Events;

namespace Cqrs.Akka.Events
{
	/// <summary>
	/// A <see cref="IEventPublisher{TAuthenticationToken}"/> that proxies <see cref="IEvent{TAuthenticationToken}"/> back onto the <see cref="IActorRef"/> and then publishes the <see cref="IEvent{TAuthenticationToken}"/> on the public event bus.
	/// </summary>
	public interface IAkkaEventPublisherProxy<TAuthenticationToken>
		: IEventPublisher<TAuthenticationToken>
	{
	}
}