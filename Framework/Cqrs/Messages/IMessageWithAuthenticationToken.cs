#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Messages
{
	public interface IMessageWithAuthenticationToken<TAuthenticationToken> : IMessage
	{
		TAuthenticationToken AuthenticationToken { get; set; }
	}
}