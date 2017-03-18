#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace Cqrs.Commands
{
	/// <summary>
	/// Publishes an <see cref="ICommand{TAuthenticationToken}"/>
	/// </summary>
	public interface ICommandPublisher<TAuthenticationToken>
	{
		void Publish<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>;

		void Publish<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>;
	}
}