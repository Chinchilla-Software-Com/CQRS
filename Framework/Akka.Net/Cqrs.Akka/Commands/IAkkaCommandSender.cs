#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Cqrs.Commands;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="ICommandSender{TAuthenticationToken}"/> that proxies <see cref="ICommand{TAuthenticationToken}"/> back onto the <see cref="IActorRef"/> and then publishes the <see cref="ICommand{TAuthenticationToken}"/> on the public command bus.
	/// </summary>
	public interface IAkkaCommandSender<TAuthenticationToken>
		: ICommandSender<TAuthenticationToken>
	{
	}
}