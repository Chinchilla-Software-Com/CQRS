#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Events
{
	public interface IEventPublisher<TAuthenticationToken>
	{
		void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>;
	}
}