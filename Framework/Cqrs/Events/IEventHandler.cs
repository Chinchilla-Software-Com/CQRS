#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Messages;

namespace Cqrs.Events
{
	/// <summary>
	/// Responds to or "Handles" a <typeparamref name="TEvent"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <typeparam name="TTarget">The <see cref="Type"/> of the object that is targeted that needs concurrency.</typeparam>
	/// <typeparam name="TEvent">The <see cref="Type"/> of <see cref="IEvent{TAuthenticationToken}"/> that can be handled.</typeparam>
	public interface IEventHandler<TAuthenticationToken, TTarget, in TEvent>
		: IEventHandler<TAuthenticationToken, TEvent>
		where TEvent : IEvent<TAuthenticationToken>
	{
	}

	/// <summary>
	/// Responds to or "Handles" a <typeparamref name="TEvent"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <typeparam name="TEvent">The <see cref="Type"/> of <see cref="IEvent{TAuthenticationToken}"/> that can be handled.</typeparam>
	public interface IEventHandler<TAuthenticationToken, in TEvent>
		: IMessageHandler<TEvent>
		, IEventHandler
		where TEvent : IEvent<TAuthenticationToken>
	{
	}

	/// <summary>
	/// Responds to or "Handles" a <see cref="IEvent{TAuthenticationToken}"/>.
	/// </summary>
	public interface IEventHandler
		: IHandler
	{
	}
}