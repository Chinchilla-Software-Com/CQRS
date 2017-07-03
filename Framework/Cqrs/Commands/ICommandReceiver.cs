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
	/// Receives instances of a <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
	/// </summary>
	public interface ICommandReceiver
	{
		/// <summary>
		/// Starts listening and processing instances of <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		void Start();
	}

	/// <summary>
	/// Receives instances of a <see cref="ICommand{TAuthenticationToken}"/> from the command bus
	/// </summary>
	public interface ICommandReceiver<TAuthenticationToken> : ICommandReceiver
	{
		/// <summary>
		/// Receives a <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		bool? ReceiveCommand(ICommand<TAuthenticationToken> command);
	}
}