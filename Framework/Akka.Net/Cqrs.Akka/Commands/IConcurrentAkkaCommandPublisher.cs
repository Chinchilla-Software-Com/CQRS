#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="IAkkaCommandPublisher{TAuthenticationToken}"/> that ensure concurrency regardless of what it passes the command onto as it is a <see cref="ReceiveActor"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <typeparam name="TTarget">The <see cref="Type"/> of the object that is targeted that needs concurrency.</typeparam>
	public interface IConcurrentAkkaCommandPublisher<TAuthenticationToken, TTarget>
		: IConcurrentAkkaCommandPublisher<TAuthenticationToken>
	{
	}

	/// <summary>
	/// A <see cref="IAkkaCommandPublisher{TAuthenticationToken}"/> that ensure concurrency regardless of what it passes the command onto as it is a <see cref="ReceiveActor"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface IConcurrentAkkaCommandPublisher<TAuthenticationToken>
		: IAkkaCommandPublisher<TAuthenticationToken>
	{
	}
}