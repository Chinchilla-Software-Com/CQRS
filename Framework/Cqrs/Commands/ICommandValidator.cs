#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Commands
{
	/// <summary>
	/// Validates an <see cref="ICommand{TAuthenticationToken}"/> on its own merits.
	/// </summary>
	public interface ICommandValidator<TAuthenticationToken, in TCommand>
		where TCommand : ICommand<TAuthenticationToken>
	{
		/// <summary>
		/// Validates the provided <param name="command" /> on its own merits.
		/// </summary>
		bool IsCommandValid(TCommand command);
	}
}