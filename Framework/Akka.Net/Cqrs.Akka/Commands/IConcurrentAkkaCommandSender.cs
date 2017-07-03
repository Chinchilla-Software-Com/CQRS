#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Akka.Commands
{
	public interface IConcurrentAkkaCommandSender<TAuthenticationToken, TTarget>
		: IConcurrentAkkaCommandSender<TAuthenticationToken>
	{
	}

	public interface IConcurrentAkkaCommandSender<TAuthenticationToken>
		: IAkkaCommandSender<TAuthenticationToken>
	{
	}
}