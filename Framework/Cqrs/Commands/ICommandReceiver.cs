#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Commands
{
	public interface ICommandReceiver<TAuthenticationToken> : ICommandReceiver
	{
		void ReceiveCommand(ICommand<TAuthenticationToken> command);
	}

	public interface ICommandReceiver
	{
		void Start();
	}
}