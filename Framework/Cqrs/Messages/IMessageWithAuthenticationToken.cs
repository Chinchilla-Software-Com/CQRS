#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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