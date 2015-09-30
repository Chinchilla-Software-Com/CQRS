#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Messages
{
	public interface IMessageHandler<in TMessage>
		: IHandler
		where TMessage: IMessage
	{
		void Handle(TMessage message);
	}
}