#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;

namespace Cqrs.Commands
{
	/// <summary>
	/// Receives instances of a <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public interface IAsyncCommandReceiver<TAuthenticationToken>
		: ICommandReceiver
	{
		/// <summary>
		/// Receives a <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		Task<bool?> ReceiveCommandAsync(ICommand<TAuthenticationToken> command);
	}
}