#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Cqrs.Commands;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="ICommandPublisher{TAuthenticationToken}"/> that proxies <see cref="ICommand{TAuthenticationToken}"/> back onto the <see cref="IActorRef"/> and then publishes the <see cref="ICommand{TAuthenticationToken}"/> on the public command bus.
	/// </summary>
	public interface IAkkaCommandPublisherProxy<TAuthenticationToken>
		: ICommandPublisher<TAuthenticationToken>
	{
	}
}