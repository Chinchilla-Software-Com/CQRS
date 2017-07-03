#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
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