#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Commands
{
	public interface ICommandReceiver
	{
		void Start();
	}

	public interface ICommandReceiver<TAuthenticationToken> : ICommandReceiver
	{
		bool? ReceiveCommand(ICommand<TAuthenticationToken> command);
	}
}