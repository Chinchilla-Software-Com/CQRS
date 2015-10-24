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
		: IAkkaCommandSender<TAuthenticationToken>
	{
	}
}