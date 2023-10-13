#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
#if NET40
#else
using System.Threading.Tasks;
#endif

namespace Cqrs.Messages
{
	/// <summary>
	/// Responds to or "Handles" a <see cref="IMessage"/>.
	/// </summary>
	/// <typeparam name="TMessage">The <see cref="Type"/> of <see cref="IMessage"/>.</typeparam>
	public interface IMessageHandler<in TMessage>
		: IHandler
		where TMessage: IMessage
	{
		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <typeparamref name="TMessage"/> to respond to or "handle"</param>
#if NET40
		void Handle
#else
		Task HandleAsync
#endif
			(TMessage message);
	}
}