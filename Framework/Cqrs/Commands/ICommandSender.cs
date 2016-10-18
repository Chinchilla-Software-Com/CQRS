#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Commands
{
	/// <summary>
	/// Sends an <see cref="ICommand{TAuthenticationToken}"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken"></typeparam>
	public interface ICommandSender<TAuthenticationToken>
	{
		void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>;
	}
}