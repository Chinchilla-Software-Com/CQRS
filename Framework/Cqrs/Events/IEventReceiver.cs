#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Events
{
	/// <summary>
	/// Receives instances of a <see cref="IEvent{TAuthenticationToken}"/> from the event bus.
	/// </summary>
	public interface IEventReceiver
	{
		/// <summary>
		/// Starts listening and processing instances of <see cref="IEvent{TAuthenticationToken}"/> from the event bus.
		/// </summary>
		void Start();
	}

	/// <summary>
	/// Receives instances of a <see cref="IEvent{TAuthenticationToken}"/> from the event bus.
	/// </summary>
	public interface IEventReceiver<TAuthenticationToken>
		: IEventReceiver
	{
		/// <summary>
		/// Receives a <see cref="IEvent{TAuthenticationToken}"/> from the event bus.
		/// </summary>
		bool? ReceiveEvent(IEvent<TAuthenticationToken> @event);
	}
}