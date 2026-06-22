#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cqrs.Commands
{
	/// <summary>
	/// Publishes an <see cref="ICommand{TAuthenticationToken}"/>
	/// </summary>
	public interface IAsyncCommandPublisher
		<TAuthenticationToken>
	{
		/// <summary>
		/// Publishes the provided <paramref name="command"/> on the command bus.
		/// </summary>
		Task PublishAsync<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Publishes the provided <paramref name="commands"/> on the command bus.
		/// </summary>
		Task PublishAsync<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Publishes the provided <paramref name="command"/> on the command bus.with a delay
		/// </summary>
		Task PublishAsync<TCommand>(TCommand command, TimeSpan delay)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Publishes the provided <paramref name="commands"/> on the command bus.with a delay.
		/// </summary>
		Task PublishAsync<TCommand>(IEnumerable<TCommand> commands, TimeSpan delay)
			where TCommand : ICommand<TAuthenticationToken>;
	}
}