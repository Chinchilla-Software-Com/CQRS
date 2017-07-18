#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

namespace Cqrs.Commands
{
	/// <summary>
	/// Sends an <see cref="ICommand{TAuthenticationToken}"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Obsolete("Replaced by ICommandPublisher<TAuthenticationToken>")]
	public interface ICommandSender<TAuthenticationToken> : ICommandPublisher<TAuthenticationToken>
	{
		[Obsolete("Replaced by ICommandPublisher<TAuthenticationToken>.Publish(TCommand)")]
		void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>;

		[Obsolete("Replaced by ICommandPublisher<TAuthenticationToken>.Publish(IEnumerable<TCommand>)")]
		void Send<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>;
	}
}