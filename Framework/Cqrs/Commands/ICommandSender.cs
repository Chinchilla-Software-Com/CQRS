#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Commands
{
	public interface ICommandSender<TAuthenticationToken>
	{
		void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>;
	}
}