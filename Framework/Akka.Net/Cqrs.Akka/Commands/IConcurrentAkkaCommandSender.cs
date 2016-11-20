#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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