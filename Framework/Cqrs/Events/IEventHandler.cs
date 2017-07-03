#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Messages;

namespace Cqrs.Events
{
	public interface IEventHandler<TAuthenticationToken, TTarget, in TEvent>
		: IEventHandler<TAuthenticationToken, TEvent>
		where TEvent : IEvent<TAuthenticationToken>
	{
	}

	public interface IEventHandler<TAuthenticationToken, in TEvent>
		: IMessageHandler<TEvent>
		, IEventHandler
		where TEvent : IEvent<TAuthenticationToken>
	{
	}

	public interface IEventHandler
		: IHandler
	{
	}
}