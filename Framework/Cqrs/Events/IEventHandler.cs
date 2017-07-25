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
	/// <typeparam name="TTarget">The <see cref="Type"/> of the object that is targeted that needs concurrency.</typeparam>
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