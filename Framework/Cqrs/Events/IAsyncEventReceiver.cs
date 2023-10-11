#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Threading.Tasks;

namespace Cqrs.Events
{
	/// <summary>
	/// Receives instances of a <see cref="IEvent{TAuthenticationToken}"/> from the event bus.
	/// </summary>
	public interface IAsyncEventReceiver<TAuthenticationToken>
		: IEventReceiver
	{
		/// <summary>
		/// Receives a <see cref="IEvent{TAuthenticationToken}"/> from the event bus.
		/// </summary>
		Task<bool?> ReceiveEventAsync(IEvent<TAuthenticationToken> @event);
	}
}