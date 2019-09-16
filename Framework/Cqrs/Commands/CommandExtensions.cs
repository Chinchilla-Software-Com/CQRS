using System;

namespace Cqrs.Commands
{
	/// <summary>
	/// A set of extension method for <see cref="ICommand{TAuthenticationToken}"/>.
	/// </summary>
	public static class CommandExtensions
	{
		/// <summary>
		/// The identity of the target object of the provided <paramref name="command"/>.
		/// </summary>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> to locate the identify from.</param>
		/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
		/// <returns><see cref="ICommandWithIdentity{TAuthenticationToken}.Rsn"/> or <see cref="ICommand{TAuthenticationToken}.Id"/>.</returns>
		public static Guid GetIdentity<TAuthenticationToken>(this ICommand<TAuthenticationToken> command)
		{
			var commandWithIdentity = command as ICommandWithIdentity<TAuthenticationToken>;
			Guid rsn = commandWithIdentity == null ? command.Id : commandWithIdentity.Rsn;
			return rsn;
		}
	}
}